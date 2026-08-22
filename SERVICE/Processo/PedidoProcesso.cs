using DOMAIN.Model.Pedido;
using REPOSITORY.Mapeadores.Pedido;

namespace SERVICE.Processo
{
    public class PedidoProcesso(IPedidoMapeador pedidoMapeador, UsuarioProcesso usuarioProcesso, ProdutoProcesso produtoProcesso)
    {
        private readonly IPedidoMapeador _pedidoMapeador = pedidoMapeador;
        private readonly UsuarioProcesso _usuarioProcesso = usuarioProcesso;
        private readonly ProdutoProcesso _produtoProcesso = produtoProcesso;

        public async Task<List<PedidoModel>> ListarPedidos(string supermercadoId)
        {
            var produtosDoSupermercado = await _produtoProcesso.ListarProdutos(supermercadoId);
            var idsProdutosDoSupermercado = produtosDoSupermercado.Select(p => p.Id).ToHashSet();

            var pedidos = await _pedidoMapeador.ListarTodosAsync();
            var pedidosDoSupermercado = pedidos
                .Where(p => p.Itens.Any(item => idsProdutosDoSupermercado.Contains(item.ProdutoId)))
                .ToList();

            await PreencherNomesClientes(pedidosDoSupermercado);

            return [.. pedidosDoSupermercado
                .OrderBy(p => StatusPedidoUtil.Prioridade(p.Status))
                .ThenByDescending(p => p.DataPedido)];
        }

        public async Task<PedidoModel?> ListarPedidoPorId(string id, string supermercadoId)
        {
            var pedido = await _pedidoMapeador.ListarPorIdAsync(id);

            if (pedido == null) return null;

            var produtosDoSupermercado = await _produtoProcesso.ListarProdutos(supermercadoId);
            var idsProdutosDoSupermercado = produtosDoSupermercado.Select(p => p.Id).ToHashSet();

            if (!pedido.Itens.Any(item => idsProdutosDoSupermercado.Contains(item.ProdutoId)))
            {
                return null;
            }

            await PreencherNomeCliente(pedido);

            return pedido;
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
