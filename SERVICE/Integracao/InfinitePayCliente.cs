using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Excecoes;
using SERVICE.Opcoes;

namespace SERVICE.Integracao
{
    public class ResultadoConsultaPagamento
    {
        public bool Pago { get; init; }

        public long ValorCentavos { get; init; }

        public long ValorPagoCentavos { get; init; }

        public int Parcelas { get; init; }

        public string? MetodoCaptura { get; init; }
    }

    public class InfinitePayCliente(HttpClient httpClient, PagamentoOpcoes opcoes)
    {
        private static readonly JsonSerializerOptions OpcoesJson = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        private readonly HttpClient _httpClient = httpClient;
        private readonly PagamentoOpcoes _opcoes = opcoes;

        public async Task<string> CriarLinkAsync(string pedidoId, long valorCentavos, string descricao, string? nomeCliente, string? emailCliente)
        {
            var corpo = new Dictionary<string, object?>
            {
                ["handle"] = Handle(),
                ["order_nsu"] = pedidoId,
                ["redirect_url"] = _opcoes.RedirectUrl,
                ["items"] = new[] { new Dictionary<string, object> { ["quantity"] = 1, ["price"] = valorCentavos, ["description"] = descricao } }
            };

            var cliente = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(nomeCliente)) cliente["name"] = nomeCliente;
            if (!string.IsNullOrWhiteSpace(emailCliente)) cliente["email"] = emailCliente;
            if (cliente.Count > 0) corpo["customer"] = cliente;

            using var documento = await PostarAsync("/links", corpo);

            if (documento.RootElement.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(url.GetString()))
            {
                return url.GetString()!;
            }

            throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O InfinitePay não devolveu o link de pagamento.");
        }

        public async Task<ResultadoConsultaPagamento> ConsultarAsync(string pedidoId, string transactionNsu, string slug)
        {
            using var documento = await PostarAsync("/payment_check", new Dictionary<string, object?>
            {
                ["handle"] = Handle(),
                ["order_nsu"] = pedidoId,
                ["transaction_nsu"] = transactionNsu,
                ["slug"] = slug
            });

            var raiz = documento.RootElement;
            bool sucesso = LerBool(raiz, "success");
            bool pago = LerBool(raiz, "paid");
            long valor = LerLong(raiz, "amount");

            return new ResultadoConsultaPagamento
            {
                Pago = sucesso && pago,
                ValorCentavos = valor,
                ValorPagoCentavos = raiz.TryGetProperty("paid_amount", out _) ? LerLong(raiz, "paid_amount") : valor,
                Parcelas = (int)Math.Max(LerLong(raiz, "installments"), 1),
                MetodoCaptura = raiz.TryGetProperty("capture_method", out var metodo) && metodo.ValueKind == JsonValueKind.String ? metodo.GetString() : null
            };
        }

        private string Handle()
        {
            var handle = _opcoes.InfinitePayHandle?.Trim().TrimStart('$');

            if (string.IsNullOrEmpty(handle))
            {
                throw new IValidExcecao(CodigoExcecao.Generico, "Pagamento online indisponível: configure 'Pagamento:InfinitePayHandle' na API.");
            }

            return handle;
        }

        private async Task<JsonDocument> PostarAsync(string caminho, Dictionary<string, object?> corpo)
        {
            HttpResponseMessage resposta;

            try
            {
                resposta = await _httpClient.PostAsJsonAsync(caminho.TrimStart('/'), corpo, OpcoesJson);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "Não foi possível falar com o InfinitePay. Tente novamente.", ex);
            }

            var conteudo = await resposta.Content.ReadAsStringAsync();

            try
            {
                var documento = JsonDocument.Parse(string.IsNullOrWhiteSpace(conteudo) ? "{}" : conteudo);

                if (!resposta.IsSuccessStatusCode && !documento.RootElement.TryGetProperty("success", out _))
                {
                    documento.Dispose();
                    throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, $"O InfinitePay recusou a requisição (HTTP {(int)resposta.StatusCode}).");
                }

                return documento;
            }
            catch (JsonException ex)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "Resposta inválida do InfinitePay.", ex);
            }
        }

        private static bool LerBool(JsonElement raiz, string propriedade)
        {
            return raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.True;
        }

        private static long LerLong(JsonElement raiz, string propriedade)
        {
            return raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.Number && valor.TryGetInt64(out var numero) ? numero : 0;
        }
    }
}
