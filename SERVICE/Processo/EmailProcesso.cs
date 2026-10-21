using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SERVICE.Opcoes;

namespace SERVICE.Processo
{
    public class EmailProcesso(HttpClient clienteHttp, EmailOpcoes opcoes, ILogger<EmailProcesso> logger)
    {
        private readonly HttpClient _clienteHttp = clienteHttp;
        private readonly EmailOpcoes _opcoes = opcoes;
        private readonly ILogger<EmailProcesso> _logger = logger;

        /// <summary>
        /// Envia o código de acesso pelo Resend. Nunca lança exceção: falhas são apenas registradas em log,
        /// para que o cadastro não dependa do envio do e-mail.
        /// </summary>
        public async Task EnviarCodigoAcessoAsync(string emailDestino, string nomeSupermercado, string codigoAcesso)
        {
            if (string.IsNullOrWhiteSpace(_opcoes.ResendApiKey) || string.IsNullOrWhiteSpace(_opcoes.Remetente))
            {
                _logger.LogWarning("Envio de e-mail não configurado (Email:ResendApiKey / Email:Remetente). Código de acesso não enviado.");
                return;
            }

            try
            {
                using var requisicao = new HttpRequestMessage(HttpMethod.Post, "emails")
                {
                    Content = JsonContent.Create(new
                    {
                        from = $"{_opcoes.NomeRemetente} <{_opcoes.Remetente}>",
                        to = new[] { emailDestino },
                        subject = "Seu código de acesso IValid",
                        html = MontarCorpoEmail(nomeSupermercado, codigoAcesso)
                    })
                };
                requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opcoes.ResendApiKey);

                using HttpResponseMessage resposta = await _clienteHttp.SendAsync(requisicao);

                if (!resposta.IsSuccessStatusCode)
                {
                    string corpo = await resposta.Content.ReadAsStringAsync();
                    _logger.LogWarning("Resend recusou o envio do código de acesso ({Status}): {Corpo}", (int)resposta.StatusCode, corpo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar o e-mail com o código de acesso.");
            }
        }

        private static string MontarCorpoEmail(string nomeSupermercado, string codigoAcesso)
        {
            string nome = WebUtility.HtmlEncode(nomeSupermercado);
            string codigo = WebUtility.HtmlEncode(codigoAcesso);

            return $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                    <h2 style="color: #E53935;">Bem-vindo ao IValid!</h2>
                    <p>Sua conta para o supermercado <strong>{nome}</strong> foi criada com sucesso.</p>
                    <p>Use o código abaixo para acessar o painel administrativo:</p>
                    <p style="font-size: 24px; font-weight: bold; letter-spacing: 2px; background: #f4f6fb; padding: 12px 20px; text-align: center; border-radius: 6px;">{codigo}</p>
                    <p style="color: #8a8fa3; font-size: 13px;">Guarde este código, ele será solicitado toda vez que você fizer login.</p>
                </div>
                """;
        }
    }
}
