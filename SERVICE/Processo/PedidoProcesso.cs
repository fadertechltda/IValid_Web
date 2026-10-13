using DOMAIN.Model.Cliente;
using DOMAIN.Model.Configuracao;
using DOMAIN.Model.Pedido;
using DOMAIN.Model.Produto;
using Excecoes;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using REPOSITORY.Mapeadores.Pedido;
using REPOSITORY.Mapeadores.Produto;
using REPOSITORY.Mapeadores.Transacao;
using SERVICE.Opcoes;

namespace SERVICE.Processo
{
    public class PedidoProcesso(
        IPedidoMapeador pedidoMapeador,
        IProdutoMapeador produtoMapeador,
        ITransacaoMapeador transacaoMapeador,
        UsuarioProcesso usuarioProcesso,
        ProdutoProcesso produtoProcesso,
        ConfiguracaoProcesso configuracaoProcesso,
        ClienteProcesso clienteProcesso,
        OngProcesso ongProcesso,
        INotificacaoProcesso notificacaoProcesso,
        PagamentoOpcoes pagamentoOpcoes,
        ILogger<PedidoProcesso> logger)
    {
        private const int LimiteItensPorPedido = 50;
        private const double Tolerancia = 0.009;

        private readonly IPedidoMapeador _pedidoMapeador = pedidoMapeador;
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;
        private readonly ITransacaoMapeador _transacaoMapeador = transacaoMapeador;
        private readonly UsuarioProcesso _usuarioProcesso = usuarioProcesso;
        private readonly ProdutoProcesso _produtoProcesso = produtoProcesso;
        private readonly ConfiguracaoProcesso _configuracaoProcesso = configuracaoProcesso;
        private readonly ClienteProcesso _clienteProcesso = clienteProcesso;
        private readonly OngProcesso _ongProcesso = ongProcesso;
        private readonly INotificacaoProcesso _notificacaoProcesso = notificacaoProcesso;
        private readonly PagamentoOpcoes _pagamentoOpcoes = pagamentoOpcoes;
        private readonly ILogger<PedidoProcesso> _logger = logger;

        public async Task<List<PedidoModel>> ListarPedidos(string supermercadoId)
        {
            var pedidosDaLoja = await _pedidoMapeador.ListarPorSupermercadoAsync(supermercadoId);
            var pedidosLegados = await ListarPedidosLegadosDaLojaAsync(supermercadoId);

            var pedidos = pedidosDaLoja
                .Concat(pedidosLegados)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            await PreencherNomesClientes(pedidos);

            return [.. pedidos
                .OrderBy(p => StatusPedidoUtil.Prioridade(p.Status))
                .ThenByDescending(p => p.DataPedido)];
        }

        public async Task<PedidoModel?> ListarPedidoPorId(string id, string supermercadoId)
        {
            var pedido = await _pedidoMapeador.ListarPorIdAsync(id);

            if (pedido == null || !await PertenceALojaAsync(pedido, supermercadoId)) return null;

            await PreencherNomeCliente(pedido);

            return pedido;
        }

        public async Task<PedidoModel> AtualizarStatusPelaLojaAsync(string id, string supermercadoId, string? novoStatusInformado)
        {
            var novoStatus = StatusPedidoUtil.Normalizar(novoStatusInformado);

            if (novoStatus is StatusPedidoUtil.Desconhecido or StatusPedidoUtil.Pendente)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Status informado é inválido.");
            }

            _ = await ListarPedidoPorId(id, supermercadoId)
                ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");

            PedidoModel atualizado = novoStatus switch
            {
                StatusPedidoUtil.Cancelado => await CancelarAsync(id),
                StatusPedidoUtil.Finalizado => await FinalizarAsync(id),
                _ => await AlterarStatusAsync(id, novoStatus)
            };

            await _notificacaoProcesso.NotificarStatusPedidoAsync(atualizado.UsuarioId, id, novoStatus);
            await PreencherNomeCliente(atualizado);

            return atualizado;
        }

        public async Task<ResumoPedidoModel> SimularAsync(string uid, CriarPedidoModel requisicao)
        {
            var itens = AgruparItens(requisicao.Itens);
            var cliente = await _clienteProcesso.ObterClienteAsync(uid);

            var produtos = await _produtoMapeador.ListarPorIdsAsync(itens.Select(i => i.ProdutoId!));
            var configuracoes = await ObterConfiguracoesAsync(produtos);

            foreach (var produto in produtos)
            {
                ProdutoProcesso.AplicarRegras(produto, ConfiguracaoDe(produto, configuracoes));
            }

            return MontarResumo(itens, produtos.ToDictionary(p => p.Id!), cliente.CashbackDisponivel, requisicao.CashbackUsado);
        }

        public async Task<PedidoModel> CriarAsync(string uid, CriarPedidoModel requisicao)
        {
            var formaPagamento = FormaPagamentoUtil.Normalizar(requisicao.FormaPagamento)
                ?? throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, $"Forma de pagamento inválida. Use: {string.Join(", ", FormaPagamentoUtil.Aceitas)}.");

            if (formaPagamento == FormaPagamentoUtil.Demonstracao && !_pagamentoOpcoes.PermitirDemonstracao)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "O pagamento de demonstração está desativado nesta API.");
            }

            var itens = AgruparItens(requisicao.Itens);
            var clienteInicial = await _clienteProcesso.ObterClienteAsync(uid);

            string? ongId = null;
            if (!string.IsNullOrWhiteSpace(requisicao.OngId) && itens.Any(i => OrigemItemUtil.EhDoacao(i.Origem)))
            {
                ongId = (await _ongProcesso.ObterAtivaAsync(requisicao.OngId)).Id;
            }

            var produtosIniciais = await _produtoMapeador.ListarPorIdsAsync(itens.Select(i => i.ProdutoId!));
            var configuracoes = await ObterConfiguracoesAsync(produtosIniciais);
            var pedidoId = _transacaoMapeador.GerarIdPedido();
            var endereco = string.IsNullOrWhiteSpace(requisicao.Endereco) ? clienteInicial.Endereco?.Formatado() : requisicao.Endereco.Trim();

            return await _transacaoMapeador.ExecutarAsync(async transacao =>
            {
                var produtos = new Dictionary<string, ProdutoModel>();

                foreach (var produtoId in itens.Select(i => i.ProdutoId!).Distinct())
                {
                    var produto = await transacao.ObterProdutoAsync(produtoId);
                    if (produto == null) continue;

                    ProdutoProcesso.AplicarRegras(produto, ConfiguracaoDe(produto, configuracoes));
                    produtos[produtoId] = produto;
                }

                var cliente = await transacao.ObterClienteAsync(uid);
                var resumo = MontarResumo(itens, produtos, cliente?.CashbackDisponivel ?? 0, requisicao.CashbackUsado);

                if (!resumo.Valido)
                {
                    throw new PedidoInvalidoExcecao(resumo);
                }

                var pedido = MontarPedido(pedidoId, uid, formaPagamento, resumo, ongId, endereco);

                foreach (var grupo in resumo.Itens.GroupBy(i => i.ProdutoId!))
                {
                    var produto = produtos[grupo.Key];
                    transacao.AtualizarProduto(grupo.Key, new Dictionary<string, object>
                    {
                        ["quantidade"] = produto.Quantidade - grupo.Sum(i => i.Quantidade)
                    });
                }

                if (resumo.CashbackAplicado > 0)
                {
                    transacao.MesclarCliente(uid, new Dictionary<string, object>
                    {
                        ["availableCashback"] = FieldValue.Increment(-resumo.CashbackAplicado)
                    });
                }

                transacao.CriarPedido(pedido);

                return pedido;
            });
        }

        public async Task<List<PedidoModel>> ListarDoClienteAsync(string uid)
        {
            var pedidos = await _pedidoMapeador.ListarPorUsuarioAsync(uid);
            return [.. pedidos.OrderByDescending(p => p.DataPedido)];
        }

        public async Task<PedidoModel> ObterDoClienteAsync(string uid, string id)
        {
            var pedido = await _pedidoMapeador.ListarPorIdAsync(id);

            if (pedido == null || pedido.UsuarioId != uid)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");
            }

            return pedido;
        }

        public async Task<PedidoModel> CancelarPeloClienteAsync(string uid, string id)
        {
            var pedido = await ObterDoClienteAsync(uid, id);

            if (StatusPedidoUtil.Normalizar(pedido.Status) == StatusPedidoUtil.Cancelado) return pedido;

            if (!StatusPedidoUtil.ClientePodeCancelar(pedido.Status))
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "O pedido já está em separação e não pode mais ser cancelado pelo aplicativo. Fale com a loja.");
            }

            return await CancelarAsync(id);
        }

        public async Task<(PedidoModel Pedido, bool Expirado)> ConfirmarPagamentoAsync(string pedidoId, Dictionary<string, object> dadosPagamento)
        {
            (PedidoModel Pedido, bool Expirado) resultado = await _transacaoMapeador.ExecutarAsync<(PedidoModel, bool)>(async transacao =>
            {
                var pedido = await transacao.ObterPedidoAsync(pedidoId)
                    ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");

                if (pedido.StatusPagamento == StatusPagamentoUtil.Pago)
                {
                    return (pedido, false);
                }

                pedido.StatusPagamento = StatusPagamentoUtil.Pago;
                pedido.Pagamento = dadosPagamento;

                var campos = new Dictionary<string, object>
                {
                    ["paymentStatus"] = StatusPagamentoUtil.Pago,
                    ["payment"] = dadosPagamento
                };

                bool expirado = StatusPedidoUtil.Normalizar(pedido.Status) != StatusPedidoUtil.Pendente;

                if (expirado)
                {
                    pedido.EstornoPendente = true;
                    campos["estornoPendente"] = true;
                }
                else
                {
                    pedido.Status = StatusPedidoUtil.Confirmado;
                    campos["status"] = StatusPedidoUtil.Confirmado;
                }

                transacao.AtualizarPedido(pedidoId, campos);
                return (pedido, expirado);
            });

            if (!resultado.Expirado)
            {
                await _notificacaoProcesso.NotificarStatusPedidoAsync(resultado.Pedido.UsuarioId, pedidoId, StatusPedidoUtil.Confirmado);
            }

            return resultado;
        }

        public async Task<int> CancelarPendentesExpiradosAsync()
        {
            var limite = DateTime.UtcNow.AddMinutes(-Math.Max(_pagamentoOpcoes.MinutosExpiracaoPedidoPendente, 5));
            var pendentes = await _pedidoMapeador.ListarPorStatusAsync(StatusPedidoUtil.Pendente);
            int cancelados = 0;

            foreach (var pedido in pendentes.Where(p => p.PagamentoOnline && p.DataPedido < limite && p.StatusPagamento != StatusPagamentoUtil.Pago))
            {
                try
                {
                    await CancelarAsync(pedido.Id!);
                    cancelados++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível cancelar o pedido pendente {PedidoId}", pedido.Id);
                }
            }

            return cancelados;
        }

        private async Task<PedidoModel> CancelarAsync(string pedidoId)
        {
            return await _transacaoMapeador.ExecutarAsync(async transacao =>
            {
                var pedido = await transacao.ObterPedidoAsync(pedidoId)
                    ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");

                if (StatusPedidoUtil.Normalizar(pedido.Status) == StatusPedidoUtil.Cancelado) return pedido;

                ValidarTransicao(pedido, StatusPedidoUtil.Cancelado);

                bool devolverEstoque = pedido.CriadoPelaApi && !pedido.EstoqueDevolvido;
                var produtos = new Dictionary<string, ProdutoModel>();

                if (devolverEstoque)
                {
                    foreach (var produtoId in pedido.Itens.Select(i => i.ProdutoId).OfType<string>().Distinct())
                    {
                        var produto = await transacao.ObterProdutoAsync(produtoId);
                        if (produto != null) produtos[produtoId] = produto;
                    }

                    foreach (var grupo in pedido.Itens.Where(i => i.ProdutoId != null && produtos.ContainsKey(i.ProdutoId)).GroupBy(i => i.ProdutoId!))
                    {
                        transacao.AtualizarProduto(grupo.Key, new Dictionary<string, object>
                        {
                            ["quantidade"] = produtos[grupo.Key].Quantidade + grupo.Sum(i => i.Quantidade)
                        });
                    }

                    if (pedido.CashbackUsado > 0 && !string.IsNullOrEmpty(pedido.UsuarioId))
                    {
                        transacao.MesclarCliente(pedido.UsuarioId, new Dictionary<string, object>
                        {
                            ["availableCashback"] = FieldValue.Increment(pedido.CashbackUsado)
                        });
                    }
                }

                bool pago = pedido.StatusPagamento == StatusPagamentoUtil.Pago;

                pedido.Status = StatusPedidoUtil.Cancelado;
                pedido.EstoqueDevolvido = pedido.EstoqueDevolvido || devolverEstoque;
                pedido.EstornoPendente = pedido.EstornoPendente || pago;
                pedido.StatusPagamento = pago ? StatusPagamentoUtil.Pago : StatusPagamentoUtil.Cancelado;

                transacao.AtualizarPedido(pedidoId, new Dictionary<string, object>
                {
                    ["status"] = pedido.Status,
                    ["estoqueDevolvido"] = pedido.EstoqueDevolvido,
                    ["estornoPendente"] = pedido.EstornoPendente,
                    ["paymentStatus"] = pedido.StatusPagamento
                });

                return pedido;
            });
        }

        private async Task<PedidoModel> FinalizarAsync(string pedidoId)
        {
            return await _transacaoMapeador.ExecutarAsync(async transacao =>
            {
                var pedido = await transacao.ObterPedidoAsync(pedidoId)
                    ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");

                ValidarTransicao(pedido, StatusPedidoUtil.Finalizado);

                var campos = new Dictionary<string, object> { ["status"] = StatusPedidoUtil.Finalizado };

                if (pedido.CriadoPelaApi && !pedido.BeneficiosAplicados && !string.IsNullOrEmpty(pedido.UsuarioId))
                {
                    var cliente = await transacao.ObterClienteAsync(pedido.UsuarioId);
                    var impacto = pedido.Impacto ?? new ImpactoModel();

                    pedido.CashbackGanho = pedido.QuantidadeItensDoacao > 0
                        ? NivelFidelidadeUtil.CalcularCashback(pedido.SubtotalDoacao, cliente?.TotalDoacoes ?? 0)
                        : 0;

                    transacao.MesclarCliente(pedido.UsuarioId, new Dictionary<string, object>
                    {
                        ["totalDonations"] = FieldValue.Increment(pedido.QuantidadeItensDoacao),
                        ["availableCashback"] = FieldValue.Increment(pedido.CashbackGanho),
                        ["impact"] = new Dictionary<string, object>
                        {
                            ["itemsRescued"] = FieldValue.Increment(impacto.ItensResgatados),
                            ["itemsDonated"] = FieldValue.Increment(impacto.ItensDoados),
                            ["savedReais"] = FieldValue.Increment(impacto.EconomiaReais),
                            ["donatedReais"] = FieldValue.Increment(impacto.DoadoReais),
                            ["methodologyVersion"] = ImpactoModel.VersaoMetodologia
                        }
                    });

                    pedido.BeneficiosAplicados = true;
                    campos["beneficiosAplicados"] = true;
                    campos["cashbackEarned"] = pedido.CashbackGanho;
                }

                if (pedido.StatusPagamento == StatusPagamentoUtil.PagarNaRetirada)
                {
                    pedido.StatusPagamento = StatusPagamentoUtil.Pago;
                    campos["paymentStatus"] = StatusPagamentoUtil.Pago;
                }

                pedido.Status = StatusPedidoUtil.Finalizado;
                transacao.AtualizarPedido(pedidoId, campos);

                return pedido;
            });
        }

        private async Task<PedidoModel> AlterarStatusAsync(string pedidoId, string novoStatus)
        {
            return await _transacaoMapeador.ExecutarAsync(async transacao =>
            {
                var pedido = await transacao.ObterPedidoAsync(pedidoId)
                    ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pedido não encontrado.");

                ValidarTransicao(pedido, novoStatus);

                pedido.Status = novoStatus;
                transacao.AtualizarPedido(pedidoId, new Dictionary<string, object> { ["status"] = novoStatus });

                return pedido;
            });
        }

        private static void ValidarTransicao(PedidoModel pedido, string novoStatus)
        {
            if (!StatusPedidoUtil.TransicaoPermitida(pedido.Status, novoStatus))
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida,
                    $"Não é possível mudar o pedido de {StatusPedidoUtil.Normalizar(pedido.Status)} para {novoStatus}.");
            }
        }

        private static List<ItemCriarPedidoModel> AgruparItens(List<ItemCriarPedidoModel>? itens)
        {
            if (itens == null || itens.Count == 0)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "O pedido precisa ter ao menos um item.");
            }

            if (itens.Any(i => string.IsNullOrWhiteSpace(i.ProdutoId) || i.Quantidade < 1 || i.Quantidade > 999))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Todo item precisa de um produto e de uma quantidade entre 1 e 999.");
            }

            var agrupados = itens
                .GroupBy(i => (ProdutoId: i.ProdutoId!.Trim(), Origem: OrigemItemUtil.EhDoacao(i.Origem) ? OrigemItemUtil.Doacao : OrigemItemUtil.Compra))
                .Select(g => new ItemCriarPedidoModel { ProdutoId = g.Key.ProdutoId, Origem = g.Key.Origem, Quantidade = g.Sum(i => i.Quantidade) })
                .ToList();

            if (agrupados.Count > LimiteItensPorPedido)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, $"O pedido pode ter no máximo {LimiteItensPorPedido} itens diferentes.");
            }

            return agrupados;
        }

        private static ResumoPedidoModel MontarResumo(List<ItemCriarPedidoModel> itens, Dictionary<string, ProdutoModel> produtos, double cashbackDisponivel, double cashbackSolicitado)
        {
            var resumo = new ResumoPedidoModel { CashbackDisponivel = Math.Round(Math.Max(cashbackDisponivel, 0), 2) };

            var lojaDoPedido = itens
                .Select(i => produtos.GetValueOrDefault(i.ProdutoId!)?.SupermercadoId)
                .FirstOrDefault(id => !string.IsNullOrEmpty(id));

            resumo.SupermercadoId = lojaDoPedido;

            foreach (var grupo in itens.GroupBy(i => i.ProdutoId!))
            {
                int solicitado = grupo.Sum(i => i.Quantidade);

                if (!produtos.TryGetValue(grupo.Key, out var produto))
                {
                    resumo.Problemas.Add(new ProblemaItemPedidoModel { ProdutoId = grupo.Key, Motivo = "Produto não encontrado.", Solicitado = solicitado });
                    continue;
                }

                string? motivo = null;

                if (string.IsNullOrEmpty(produto.SupermercadoId)) motivo = "Produto sem loja vinculada.";
                else if (produto.SupermercadoId != lojaDoPedido) motivo = "Um pedido só pode ter produtos de uma loja.";
                else if (string.Equals(produto.Status, "VENCIDO", StringComparison.OrdinalIgnoreCase)) motivo = "Produto vencido.";
                else if (produto.Quantidade < solicitado) motivo = "Estoque insuficiente.";

                if (motivo != null)
                {
                    resumo.Problemas.Add(new ProblemaItemPedidoModel
                    {
                        ProdutoId = grupo.Key,
                        Nome = produto.Nome,
                        Motivo = motivo,
                        Solicitado = solicitado,
                        Disponivel = Math.Max(produto.Quantidade, 0)
                    });
                }

                foreach (var item in grupo)
                {
                    resumo.Itens.Add(new ItemPedidoModel
                    {
                        ProdutoId = produto.Id,
                        NomeProduto = produto.Nome,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = Math.Round(produto.PrecoPromocao, 2),
                        PrecoUnitarioOriginal = Math.Round(produto.Preco, 2),
                        Subtotal = Math.Round(produto.PrecoPromocao * item.Quantidade, 2),
                        Origem = item.Origem,
                        EhDoacao = OrigemItemUtil.EhDoacao(item.Origem)
                    });
                }
            }

            resumo.Subtotal = Math.Round(resumo.Itens.Sum(i => i.Subtotal), 2);
            resumo.CashbackMaximoPermitido = NivelFidelidadeUtil.CashbackMaximoPermitido(resumo.Subtotal, resumo.CashbackDisponivel);
            resumo.CashbackAplicado = Math.Round(Math.Min(Math.Max(cashbackSolicitado, 0), resumo.CashbackMaximoPermitido), 2);
            resumo.Total = Math.Round(Math.Max(resumo.Subtotal - resumo.CashbackAplicado, 0), 2);

            return resumo;
        }

        private PedidoModel MontarPedido(string pedidoId, string uid, string formaPagamento, ResumoPedidoModel resumo, string? ongId, string? endereco)
        {
            var itensDoacao = resumo.Itens.Where(i => i.EhDoacao).ToList();
            var itensCompra = resumo.Itens.Where(i => !i.EhDoacao).ToList();

            var pedido = new PedidoModel
            {
                Id = pedidoId,
                UsuarioId = uid,
                SupermercadoId = resumo.SupermercadoId,
                Endereco = endereco,
                Itens = resumo.Itens,
                Subtotal = resumo.Subtotal,
                CashbackUsado = resumo.CashbackAplicado,
                Total = resumo.Total,
                PossuiDoacoes = itensDoacao.Count > 0,
                QuantidadeItensDoacao = itensDoacao.Sum(i => i.Quantidade),
                SubtotalDoacao = Math.Round(itensDoacao.Sum(i => i.Subtotal), 2),
                OngId = ongId,
                DataPedido = DateTime.UtcNow,
                CriadoPelaApi = true,
                Impacto = new ImpactoModel
                {
                    ItensResgatados = itensCompra.Sum(i => i.Quantidade),
                    ItensDoados = itensDoacao.Sum(i => i.Quantidade),
                    EconomiaReais = Math.Round(itensCompra.Sum(i => Math.Max(i.PrecoUnitarioOriginal - i.PrecoUnitario, 0) * i.Quantidade), 2),
                    DoadoReais = Math.Round(itensDoacao.Sum(i => i.Subtotal), 2)
                }
            };

            if (resumo.Total <= Tolerancia)
            {
                pedido.FormaPagamento = FormaPagamentoUtil.Cashback;
                pedido.Status = StatusPedidoUtil.Confirmado;
                pedido.StatusPagamento = StatusPagamentoUtil.Pago;
                return pedido;
            }

            pedido.FormaPagamento = formaPagamento;

            switch (formaPagamento)
            {
                case FormaPagamentoUtil.Cashback:
                    throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "O cashback disponível não cobre o total do pedido. Escolha outra forma de pagamento.");
                case FormaPagamentoUtil.Online:
                    pedido.PagamentoOnline = true;
                    pedido.Status = StatusPedidoUtil.Pendente;
                    pedido.StatusPagamento = StatusPagamentoUtil.AguardandoPagamento;
                    break;
                case FormaPagamentoUtil.Demonstracao:
                    pedido.Status = StatusPedidoUtil.Confirmado;
                    pedido.StatusPagamento = StatusPagamentoUtil.Pago;
                    pedido.Pagamento = new Dictionary<string, object> { ["demonstracao"] = true };
                    break;
                default:
                    pedido.Status = StatusPedidoUtil.Confirmado;
                    pedido.StatusPagamento = StatusPagamentoUtil.PagarNaRetirada;
                    break;
            }

            return pedido;
        }

        private async Task<Dictionary<string, ConfiguracaoModel>> ObterConfiguracoesAsync(IEnumerable<ProdutoModel> produtos)
        {
            var configuracoes = new Dictionary<string, ConfiguracaoModel>();

            foreach (var supermercadoId in produtos.Select(p => p.SupermercadoId).OfType<string>().Distinct())
            {
                configuracoes[supermercadoId] = await _configuracaoProcesso.ObterConfiguracao(supermercadoId);
            }

            return configuracoes;
        }

        private static ConfiguracaoModel ConfiguracaoDe(ProdutoModel produto, Dictionary<string, ConfiguracaoModel> configuracoes)
        {
            return produto.SupermercadoId != null && configuracoes.TryGetValue(produto.SupermercadoId, out var configuracao)
                ? configuracao
                : new ConfiguracaoModel { SupermercadoId = produto.SupermercadoId };
        }

        private async Task<List<PedidoModel>> ListarPedidosLegadosDaLojaAsync(string supermercadoId)
        {
            var produtosDoSupermercado = await _produtoProcesso.ListarProdutos(supermercadoId);
            var idsProdutos = produtosDoSupermercado.Select(p => p.Id).ToHashSet();

            var pedidosSemLoja = await _pedidoMapeador.ListarSemSupermercadoAsync();

            return [.. pedidosSemLoja.Where(p => p.Itens.Any(item => idsProdutos.Contains(item.ProdutoId)))];
        }

        private async Task<bool> PertenceALojaAsync(PedidoModel pedido, string supermercadoId)
        {
            if (!string.IsNullOrEmpty(pedido.SupermercadoId))
            {
                return pedido.SupermercadoId == supermercadoId;
            }

            var produtosDoSupermercado = await _produtoProcesso.ListarProdutos(supermercadoId);
            var idsProdutos = produtosDoSupermercado.Select(p => p.Id).ToHashSet();

            return pedido.Itens.Any(item => idsProdutos.Contains(item.ProdutoId));
        }

        private async Task PreencherNomesClientes(List<PedidoModel> pedidos)
        {
            var nomesPorUsuario = new Dictionary<string, string?>();

            foreach (var pedido in pedidos)
            {
                if (string.IsNullOrEmpty(pedido.UsuarioId)) continue;

                if (!nomesPorUsuario.TryGetValue(pedido.UsuarioId, out var nome))
                {
                    var usuario = await _usuarioProcesso.ObterPorIdAsync(pedido.UsuarioId);
                    nome = usuario?.NomeCompleto;
                    nomesPorUsuario[pedido.UsuarioId] = nome;
                }

                pedido.ClienteNome = nome;
            }
        }

        private async Task PreencherNomeCliente(PedidoModel pedido)
        {
            if (string.IsNullOrEmpty(pedido.UsuarioId)) return;

            var usuario = await _usuarioProcesso.ObterPorIdAsync(pedido.UsuarioId);
            pedido.ClienteNome = usuario?.NomeCompleto;
        }
    }
}
