using System.Text;
using System.Text.Json;
using Excecoes;

namespace WEB.Controllers
{
    public static class CadastroApiHelper
    {
        private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

        public static StringContent ComoJson(object valor) => new(JsonSerializer.Serialize(valor), Encoding.UTF8, "application/json");

        public static async Task<T?> LerAsync<T>(HttpResponseMessage resposta)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(conteudo, OpcoesJson);
        }

        public static async Task<string> ExtrairErroAsync(HttpResponseMessage resposta, string padrao)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ExcecaoDetalhes>(conteudo, OpcoesJson)?.InformacaoAdicional ?? padrao;
            }
            catch (JsonException)
            {
                return padrao;
            }
        }
    }
}
