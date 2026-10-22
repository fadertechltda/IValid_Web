using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using SERVICE.Integracao;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers.App
{
    [AllowAnonymous]
    [Route("api/app/pagamentos/mercadopago/webhook")]
    [Tags("App - Pagamentos")]
    public class MercadoPagoWebhookController(PedidoFachada pedidoFachada) : ControllerApiBase
    {
        private readonly PedidoFachada _pedidoFachada = pedidoFachada;

        /// <summary>Notificação do Mercado Pago. Só o identificador é lido; o estado real é consultado na API do Mercado Pago.</summary>
        [HttpPost]
        public Task<IActionResult> Receber([FromBody] JsonElement corpo) =>
            ExecutarAsync(async () =>
            {
                string? ordemId = LerOrdemId(corpo);

                if (!MercadoPagoCliente.IdValido(ordemId))
                {
                    return Ok();
                }

                await _pedidoFachada.ProcessarNotificacaoMercadoPagoAsync(ordemId!);
                return Ok();
            });

        private static string? LerOrdemId(JsonElement corpo)
        {
            if (corpo.ValueKind != JsonValueKind.Object) return null;

            if (corpo.TryGetProperty("data", out var dados) && dados.ValueKind == JsonValueKind.Object
                && dados.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            {
                return id.GetString();
            }

            return null;
        }
    }
}
