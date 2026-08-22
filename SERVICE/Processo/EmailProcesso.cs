using System.Net;
using System.Net.Mail;

namespace SERVICE.Processo
{
    public class EmailProcesso(string? host, int porta, string? emailRemetente, string? senhaRemetente, string? nomeRemetente)
    {
        private readonly string? _host = host;
        private readonly int _porta = porta;
        private readonly string? _emailRemetente = emailRemetente;
        private readonly string? _senhaRemetente = senhaRemetente;
        private readonly string? _nomeRemetente = nomeRemetente;

        public async Task EnviarCodigoAcessoAsync(string emailDestino, string nomeSupermercado, string codigoAcesso)
        {
            if (string.IsNullOrEmpty(_host) || string.IsNullOrEmpty(_emailRemetente))
            {
                throw new InvalidOperationException("O envio de email não está configurado (seção 'Email' ausente no appsettings.json).");
            }

            using var mensagem = new MailMessage
            {
                From = new MailAddress(_emailRemetente, _nomeRemetente ?? "IValid"),
                Subject = "Seu código de acesso IValid",
                Body = MontarCorpoEmail(nomeSupermercado, codigoAcesso),
                IsBodyHtml = true
            };
            mensagem.To.Add(emailDestino);

            using var clienteSmtp = new SmtpClient(_host, _porta)
            {
                Credentials = new NetworkCredential(_emailRemetente, _senhaRemetente),
                EnableSsl = true
            };

            await clienteSmtp.SendMailAsync(mensagem);
        }

        private static string MontarCorpoEmail(string nomeSupermercado, string codigoAcesso)
        {
            return $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                    <h2 style="color: #E53935;">Bem-vindo ao IValid!</h2>
                    <p>Sua conta para o supermercado <strong>{nomeSupermercado}</strong> foi criada com sucesso.</p>
                    <p>Use o código abaixo para acessar o painel administrativo:</p>
                    <p style="font-size: 24px; font-weight: bold; letter-spacing: 2px; background: #f4f6fb; padding: 12px 20px; text-align: center; border-radius: 6px;">{codigoAcesso}</p>
                    <p style="color: #8a8fa3; font-size: 13px;">Guarde este código, ele será solicitado toda vez que você fizer login.</p>
                </div>
                """;
        }
    }
}
