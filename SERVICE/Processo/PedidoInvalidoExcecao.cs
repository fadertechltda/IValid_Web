using DOMAIN.Model.Pedido;
using Excecoes;

namespace SERVICE.Processo
{
    public class PedidoInvalidoExcecao(ResumoPedidoModel resumo)
        : IValidExcecao(CodigoExcecao.EstoqueInsuficiente, "Alguns itens do pedido não estão disponíveis.")
    {
        public ResumoPedidoModel Resumo { get; } = resumo;
    }
}
