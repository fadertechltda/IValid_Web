using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Excecoes;
using Microsoft.Extensions.Logging;
using SERVICE.Opcoes;

namespace SERVICE.Integracao
{
    public class OrdemMercadoPago
    {
        public string? Id { get; init; }

        public string? ReferenciaExterna { get; init; }

        public string? Status { get; init; }

        public string? DetalheStatus { get; init; }

        public long ValorCentavos { get; init; }

        public string? QrCode { get; init; }

        public string? QrCodeBase64 { get; init; }

        public string? TicketUrl { get; init; }

        public string? MetodoId { get; init; }

        public bool Pago { get; init; }

        public bool Falhou { get; init; }
    }

    public class DadosCartaoOrdem
    {
        public required string Token { get; init; }

        public required string BandeiraId { get; init; }

        public required string Tipo { get; init; }

        public int Parcelas { get; init; } = 1;
    }

    public partial class MercadoPagoCliente(HttpClient httpClient, MercadoPagoOpcoes opcoes, ILogger<MercadoPagoCliente> logger)
    {
        private static readonly string[] StatusDeFalha = ["failed", "canceled", "cancelled", "expired", "refunded", "rejected"];

        private readonly HttpClient _httpClient = httpClient;
        private readonly MercadoPagoOpcoes _opcoes = opcoes;
        private readonly ILogger<MercadoPagoCliente> _logger = logger;

        public Task<OrdemMercadoPago> CriarOrdemCartaoAsync(string pedidoId, long valorCentavos, string email, DadosCartaoOrdem cartao)
        {
            var pagamento = new Dictionary<string, object?>
            {
                ["amount"] = Valor(valorCentavos),
                ["payment_method"] = new Dictionary<string, object?>
                {
                    ["id"] = cartao.BandeiraId,
                    ["type"] = cartao.Tipo,
                    ["token"] = cartao.Token,
                    ["installments"] = cartao.Parcelas
                }
            };

            return CriarOrdemAsync(pedidoId, valorCentavos, email, null, pagamento, $"{pedidoId}:cartao:{cartao.Token}");
        }

        public Task<OrdemMercadoPago> CriarOrdemPixAsync(string pedidoId, long valorCentavos, string email, int minutosValidade)
        {
            var pagamento = new Dictionary<string, object?>
            {
                ["amount"] = Valor(valorCentavos),
                ["payment_method"] = new Dictionary<string, object?>
                {
                    ["id"] = "pix",
                    ["type"] = "bank_transfer"
                },
                ["expiration_time"] = $"PT{Math.Max(minutosValidade, 30)}M"
            };

            var nomeTeste = _opcoes.NomePagadorTeste?.Trim();
            return CriarOrdemAsync(pedidoId, valorCentavos, email, string.IsNullOrEmpty(nomeTeste) ? null : nomeTeste, pagamento, $"{pedidoId}:pix");
        }

        public async Task<OrdemMercadoPago> ConsultarOrdemAsync(string ordemId)
        {
            if (!IdValido(ordemId))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Identificador de pagamento inválido.");
            }

            using var requisicao = Montar(HttpMethod.Get, $"v1/orders/{Uri.EscapeDataString(ordemId)}", null, null);
            using var documento = await EnviarAsync(requisicao);
            return Converter(documento.RootElement);
        }

        public static bool IdValido(string? id) => !string.IsNullOrWhiteSpace(id) && IdRegex().IsMatch(id);

        private async Task<OrdemMercadoPago> CriarOrdemAsync(string pedidoId, long valorCentavos, string email, string? primeiroNome, Dictionary<string, object?> pagamento, string chaveIdempotencia)
        {
            var pagador = new Dictionary<string, object?> { ["email"] = email };
            if (primeiroNome != null) pagador["first_name"] = primeiroNome;

            var corpo = new Dictionary<string, object?>
            {
                ["type"] = "online",
                ["processing_mode"] = "automatic",
                ["total_amount"] = Valor(valorCentavos),
                ["external_reference"] = pedidoId,
                ["payer"] = pagador,
                ["transactions"] = new Dictionary<string, object?> { ["payments"] = new[] { pagamento } }
            };

            using var requisicao = Montar(HttpMethod.Post, "v1/orders", JsonSerializer.Serialize(corpo), chaveIdempotencia);
            using var documento = await EnviarAsync(requisicao);
            return Converter(documento.RootElement);
        }

        private HttpRequestMessage Montar(HttpMethod metodo, string caminho, string? json, string? chaveIdempotencia)
        {
            var token = _opcoes.AccessToken?.Trim();

            if (string.IsNullOrEmpty(token))
            {
                throw new IValidExcecao(CodigoExcecao.Generico, "Pagamento online indisponível: configure 'MercadoPago:AccessToken' na API.");
            }

            var requisicao = new HttpRequestMessage(metodo, caminho);
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!string.IsNullOrEmpty(chaveIdempotencia))
            {
                requisicao.Headers.Add("X-Idempotency-Key", ChaveCurta(chaveIdempotencia));
            }

            if (json != null)
            {
                requisicao.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return requisicao;
        }

        private async Task<JsonDocument> EnviarAsync(HttpRequestMessage requisicao)
        {
            HttpResponseMessage resposta;

            try
            {
                resposta = await _httpClient.SendAsync(requisicao);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "Não foi possível falar com o Mercado Pago. Tente novamente.", ex);
            }

            using (resposta)
            {
                var conteudo = await resposta.Content.ReadAsStringAsync();

                if (!resposta.IsSuccessStatusCode)
                {
                    // HTTP 402 é como o Mercado Pago avisa que o pagamento foi recusado: é um resultado
                    // normal (a ordem vem com status "failed"), não uma falha da requisição.
                    if ((int)resposta.StatusCode == 402 && TentarLerOrdem(conteudo, out var recusada))
                    {
                        return recusada;
                    }

                    _logger.LogWarning("Mercado Pago respondeu HTTP {Status} em {Metodo} {Caminho}: {Corpo}", (int)resposta.StatusCode, requisicao.Method, requisicao.RequestUri, Resumir(conteudo));

                    var mensagem = (int)resposta.StatusCode == 429
                        ? "O Mercado Pago está ocupado. Tente novamente em instantes."
                        : $"O Mercado Pago recusou a requisição (HTTP {(int)resposta.StatusCode}). Confira os dados e tente novamente.";

                    throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, mensagem);
                }

                try
                {
                    return JsonDocument.Parse(string.IsNullOrWhiteSpace(conteudo) ? "{}" : conteudo);
                }
                catch (JsonException ex)
                {
                    throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "Resposta inválida do Mercado Pago.", ex);
                }
            }
        }

        /// <summary>
        /// A resposta de recusa (HTTP 402) traz a ordem dentro de "data" e os motivos em "errors".
        /// Devolve um documento cuja raiz é a própria ordem, para o mesmo parser dos casos de sucesso.
        /// </summary>
        private static bool TentarLerOrdem(string conteudo, out JsonDocument documento)
        {
            documento = null!;

            try
            {
                using var bruto = JsonDocument.Parse(conteudo);
                var raiz = bruto.RootElement;

                if (raiz.ValueKind != JsonValueKind.Object) return false;

                if (raiz.TryGetProperty("data", out var dados) && dados.ValueKind == JsonValueKind.Object && dados.TryGetProperty("status", out _))
                {
                    documento = JsonDocument.Parse(dados.GetRawText());
                    return true;
                }

                if (raiz.TryGetProperty("status", out _))
                {
                    documento = JsonDocument.Parse(raiz.GetRawText());
                    return true;
                }
            }
            catch (JsonException)
            {
            }

            return false;
        }

        private static OrdemMercadoPago Converter(JsonElement raiz)
        {
            JsonElement pagamento = default;
            bool temPagamento = raiz.TryGetProperty("transactions", out var transacoes)
                && transacoes.ValueKind == JsonValueKind.Object
                && transacoes.TryGetProperty("payments", out var pagamentos)
                && pagamentos.ValueKind == JsonValueKind.Array
                && pagamentos.GetArrayLength() > 0
                && (pagamento = pagamentos[0]).ValueKind == JsonValueKind.Object;

            string? statusOrdem = Texto(raiz, "status");
            string? detalheOrdem = Texto(raiz, "status_detail");
            string? statusPagamento = temPagamento ? Texto(pagamento, "status") : null;
            string? detalhePagamento = temPagamento ? Texto(pagamento, "status_detail") : null;

            string? status = statusPagamento ?? statusOrdem;
            string? detalhe = detalhePagamento ?? detalheOrdem;

            bool pago = string.Equals(status, "processed", StringComparison.OrdinalIgnoreCase)
                && string.Equals(detalhe, "accredited", StringComparison.OrdinalIgnoreCase);

            bool falhou = StatusDeFalha.Contains(status?.ToLowerInvariant() ?? string.Empty);

            return new OrdemMercadoPago
            {
                Id = Texto(raiz, "id"),
                ReferenciaExterna = Texto(raiz, "external_reference"),
                Status = status,
                DetalheStatus = detalhe,
                ValorCentavos = EmCentavos(Texto(raiz, "total_amount")),
                QrCode = Buscar(raiz, "qr_code"),
                QrCodeBase64 = Buscar(raiz, "qr_code_base64"),
                TicketUrl = Buscar(raiz, "ticket_url"),
                MetodoId = Buscar(raiz, "id", "payment_method"),
                Pago = pago,
                Falhou = falhou
            };
        }

        private static string? Texto(JsonElement elemento, string propriedade)
        {
            if (!elemento.TryGetProperty(propriedade, out var valor)) return null;

            return valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number => valor.GetRawText(),
                _ => null
            };
        }

        private static string? Buscar(JsonElement elemento, string propriedade, string? dentroDe = null)
        {
            if (elemento.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in elemento.EnumerateObject())
                {
                    if (dentroDe != null)
                    {
                        if (item.Name == dentroDe && item.Value.ValueKind == JsonValueKind.Object)
                        {
                            var interno = Texto(item.Value, propriedade);
                            if (interno != null) return interno;
                        }
                    }
                    else if (item.Name == propriedade && item.Value.ValueKind == JsonValueKind.String)
                    {
                        return item.Value.GetString();
                    }

                    var achado = Buscar(item.Value, propriedade, dentroDe);
                    if (achado != null) return achado;
                }
            }
            else if (elemento.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in elemento.EnumerateArray())
                {
                    var achado = Buscar(item, propriedade, dentroDe);
                    if (achado != null) return achado;
                }
            }

            return null;
        }

        private static string Valor(long centavos) => (centavos / 100m).ToString("0.00", CultureInfo.InvariantCulture);

        private static long EmCentavos(string? valor)
        {
            return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero)
                ? (long)Math.Round(numero * 100m, MidpointRounding.AwayFromZero)
                : 0;
        }

        private static string ChaveCurta(string chave) => chave.Length <= 64 ? chave : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(chave)));

        private static string Resumir(string conteudo) => conteudo.Length <= 400 ? conteudo : conteudo[..400];

        [GeneratedRegex("^[A-Za-z0-9_-]{5,64}$")]
        private static partial Regex IdRegex();
    }
}
