using DOMAIN.Model.Pedido;
using Excecoes;
using Microsoft.Extensions.Logging;
using SERVICE.Integracao;
using SERVICE.Opcoes;

namespace SERVICE.Processo
{
    public class PagamentoProcesso(InfinitePayCliente infinitePayCliente, MercadoPagoCliente mercadoPagoCliente, PedidoProcesso pedidoProcesso, PagamentoOpcoes opcoes, MercadoPagoOpcoes mercadoPagoOpcoes, ILogger<PagamentoProcesso> logger)
    {
        private const int TentativasConsulta = 3;
        private static readonly TimeSpan IntervaloConsulta = TimeSpan.FromSeconds(2);

        private readonly InfinitePayCliente _infinitePayCliente = infinitePayCliente;
        private readonly MercadoPagoCliente _mercadoPagoCliente = mercadoPagoCliente;
        private readonly PagamentoOpcoes _opcoes = opcoes;
        private readonly MercadoPagoOpcoes _mercadoPagoOpcoes = mercadoPagoOpcoes;
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;
        private readonly ILogger<PagamentoProcesso> _logger = logger;

        public async Task<LinkPagamentoModel> GerarLinkAsync(string uid, string pedidoId, string? nomeCliente, string? emailCliente)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (!pedido.PagamentoOnline || StatusPedidoUtil.Normalizar(pedido.Status) != StatusPedidoUtil.Pendente || pedido.StatusPagamento != StatusPagamentoUtil.AguardandoPagamento)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Este pedido não está aguardando pagamento online.");
            }

            int quantidadeItens = pedido.Itens.Sum(i => i.Quantidade);
            string descricao = $"Pedido IValid ({quantidadeItens} {(quantidadeItens == 1 ? "item" : "itens")})";

            var url = await _infinitePayCliente.CriarLinkAsync(pedidoId, EmCentavos(pedido.Total), descricao, nomeCliente, emailCliente);

            return new LinkPagamentoModel { PedidoId = pedidoId, Url = url, Valor = pedido.Total };
        }

        public async Task<PedidoModel> ConfirmarAsync(string uid, string pedidoId, ConfirmarPagamentoModel dados)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (pedido.StatusPagamento == StatusPagamentoUtil.Pago) return pedido;

            if (!pedido.PagamentoOnline)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Este pedido não usa pagamento online.");
            }

            ResultadoConsultaPagamento? resultado = null;

            for (int tentativa = 1; tentativa <= TentativasConsulta; tentativa++)
            {
                resultado = await _infinitePayCliente.ConsultarAsync(pedidoId, dados.TransactionNsu!, dados.Slug!);
                if (resultado.Pago) break;
                if (tentativa < TentativasConsulta) await Task.Delay(IntervaloConsulta);
            }

            if (resultado == null || !resultado.Pago)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pagamento ainda não foi confirmado pelo InfinitePay. Tente novamente em instantes.");
            }

            long esperado = EmCentavos(pedido.Total);

            if (resultado.ValorPagoCentavos < esperado)
            {
                _logger.LogWarning("Pagamento do pedido {PedidoId} com valor divergente: esperado {Esperado}, pago {Pago}", pedidoId, esperado, resultado.ValorPagoCentavos);
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O valor pago não confere com o total do pedido. Procure a loja.");
            }

            var dadosPagamento = new Dictionary<string, object>
            {
                ["provedor"] = "InfinitePay",
                ["transactionNsu"] = dados.TransactionNsu!,
                ["slug"] = dados.Slug!,
                ["amountCents"] = resultado.ValorCentavos,
                ["paidAmountCents"] = resultado.ValorPagoCentavos,
                ["installments"] = resultado.Parcelas,
                ["captureMethod"] = resultado.MetodoCaptura ?? dados.CaptureMethod ?? string.Empty,
                ["receiptUrl"] = dados.ReceiptUrl ?? string.Empty,
                ["confirmadoEm"] = DateTime.UtcNow.ToString("O")
            };

            var (pedidoAtualizado, expirado) = await _pedidoProcesso.ConfirmarPagamentoAsync(pedidoId, dadosPagamento);

            if (expirado)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pedido expirou antes da confirmação do pagamento. A loja fará o estorno.");
            }

            return pedidoAtualizado;
        }

        public string ObterChavePublicaMercadoPago()
        {
            var chave = _mercadoPagoOpcoes.PublicKey?.Trim();

            if (string.IsNullOrEmpty(chave))
            {
                throw new IValidExcecao(CodigoExcecao.Generico, "Pagamento online indisponível: configure 'MercadoPago:PublicKey' na API.");
            }

            return chave;
        }

        public async Task<ResultadoPagamentoModel> PagarAsync(string uid, string pedidoId, string? email, PagarPedidoModel dados)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (!pedido.PagamentoOnline || StatusPedidoUtil.Normalizar(pedido.Status) != StatusPedidoUtil.Pendente || pedido.StatusPagamento != StatusPagamentoUtil.AguardandoPagamento)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Este pedido não está aguardando pagamento online.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Sua conta precisa de um e-mail para pagar online.");
            }

            long valor = EmCentavos(pedido.Total);
            string metodo = (dados.Metodo ?? string.Empty).Trim().ToLowerInvariant();
            OrdemMercadoPago ordem;

            if (metodo == MetodoPagamentoOnline.Pix)
            {
                ordem = await _mercadoPagoCliente.CriarOrdemPixAsync(pedidoId, valor, email, _opcoes.MinutosExpiracaoPedidoPendente);
            }
            else if (metodo == MetodoPagamentoOnline.Cartao)
            {
                if (string.IsNullOrWhiteSpace(dados.TokenCartao) || string.IsNullOrWhiteSpace(dados.BandeiraId))
                {
                    throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Dados do cartão incompletos.");
                }

                string tipo = string.IsNullOrWhiteSpace(dados.TipoCartao) ? "credit_card" : dados.TipoCartao.Trim().ToLowerInvariant();

                if (tipo is not ("credit_card" or "debit_card"))
                {
                    throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Tipo de cartão inválido.");
                }

                ordem = await _mercadoPagoCliente.CriarOrdemCartaoAsync(pedidoId, valor, email, new DadosCartaoOrdem
                {
                    Token = dados.TokenCartao.Trim(),
                    BandeiraId = dados.BandeiraId.Trim().ToLowerInvariant(),
                    Tipo = tipo,
                    Parcelas = tipo == "debit_card" ? 1 : dados.Parcelas
                });
            }
            else
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Método de pagamento inválido. Use 'pix' ou 'cartao'.");
            }

            if (ordem.Falhou)
            {
                _logger.LogInformation("Pagamento do pedido {PedidoId} recusado: {Status}/{Detalhe}", pedidoId, ordem.Status, ordem.DetalheStatus);
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, MensagemDeRecusa(ordem.DetalheStatus));
            }

            if (ordem.Pago)
            {
                await ConfirmarOrdemAsync(pedido, ordem, aceitarExpirado: false);
            }

            return Resultado(pedido, ordem);
        }

        public async Task<ResultadoPagamentoModel> SincronizarAsync(string uid, string pedidoId, string ordemId)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (pedido.StatusPagamento == StatusPagamentoUtil.Pago)
            {
                return new ResultadoPagamentoModel { PedidoId = pedidoId, OrdemId = ordemId, Situacao = "pago", Pago = true, Valor = pedido.Total };
            }

            var ordem = await _mercadoPagoCliente.ConsultarOrdemAsync(ordemId);

            if (ordem.ReferenciaExterna != pedidoId)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Pagamento não encontrado para este pedido.");
            }

            if (ordem.Pago)
            {
                await ConfirmarOrdemAsync(pedido, ordem, aceitarExpirado: false);
            }
            else if (ordem.Falhou)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pagamento não foi concluído. Gere um novo pagamento.");
            }

            return Resultado(pedido, ordem);
        }

        /// <summary>Chamado pelo webhook: o conteúdo recebido nunca é confiado, a ordem é sempre consultada no Mercado Pago.</summary>
        public async Task ProcessarNotificacaoOrdemAsync(string ordemId)
        {
            var ordem = await _mercadoPagoCliente.ConsultarOrdemAsync(ordemId);

            if (!ordem.Pago || string.IsNullOrWhiteSpace(ordem.ReferenciaExterna)) return;

            var pedido = await _pedidoProcesso.ObterPorIdAsync(ordem.ReferenciaExterna);

            if (pedido.StatusPagamento == StatusPagamentoUtil.Pago) return;

            await ConfirmarOrdemAsync(pedido, ordem, aceitarExpirado: true);
        }

        private async Task ConfirmarOrdemAsync(PedidoModel pedido, OrdemMercadoPago ordem, bool aceitarExpirado)
        {
            long esperado = EmCentavos(pedido.Total);

            if (ordem.ReferenciaExterna != pedido.Id || ordem.ValorCentavos < esperado)
            {
                _logger.LogWarning("Pagamento do pedido {PedidoId} divergente: esperado {Esperado}, ordem {Valor}, referência {Referencia}", pedido.Id, esperado, ordem.ValorCentavos, ordem.ReferenciaExterna);
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O valor pago não confere com o total do pedido. Procure a loja.");
            }

            var dadosPagamento = new Dictionary<string, object>
            {
                ["provedor"] = "MercadoPago",
                ["ordemId"] = ordem.Id ?? string.Empty,
                ["metodo"] = ordem.MetodoId ?? string.Empty,
                ["amountCents"] = ordem.ValorCentavos,
                ["confirmadoEm"] = DateTime.UtcNow.ToString("O")
            };

            var (_, expirado) = await _pedidoProcesso.ConfirmarPagamentoAsync(pedido.Id!, dadosPagamento);

            if (expirado && !aceitarExpirado)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pedido expirou antes da confirmação do pagamento. A loja fará o estorno.");
            }
        }

        /// <summary>Texto para o cliente a partir do motivo da recusa (o Mercado Pago não deve ser repetido ao pé da letra).</summary>
        private static string MensagemDeRecusa(string? detalhe)
        {
            string d = (detalhe ?? string.Empty).ToLowerInvariant();

            if (d.Contains("insufficient") || d.Contains("fund")) return "Pagamento recusado: saldo ou limite insuficiente. Tente outro cartão ou use o Pix.";
            if (d.Contains("security_code") || d.Contains("cvv")) return "Pagamento recusado: código de segurança (CVV) inválido. Confira e tente novamente.";
            if (d.Contains("expir") || d.Contains("date")) return "Pagamento recusado: confira a validade do cartão.";
            if (d.Contains("call_for_authorize") || d.Contains("authorize")) return "Pagamento recusado: seu banco pede autorização. Fale com o banco ou use outro cartão.";

            return "Pagamento recusado pelo banco. Tente outro cartão ou use o Pix.";
        }

        private static ResultadoPagamentoModel Resultado(PedidoModel pedido, OrdemMercadoPago ordem) => new()
        {
            PedidoId = pedido.Id,
            OrdemId = ordem.Id,
            Situacao = ordem.Pago ? "pago" : "aguardando",
            Pago = ordem.Pago,
            Valor = pedido.Total,
            QrCode = ordem.QrCode,
            QrCodeBase64 = ordem.QrCodeBase64,
            TicketUrl = ordem.TicketUrl
        };

        private static long EmCentavos(double valor) => (long)Math.Round(valor * 100, MidpointRounding.AwayFromZero);
    }
}
