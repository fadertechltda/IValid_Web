using SERVICE.Processo;

namespace WEB_API.Infraestrutura
{
    public class RotinaDiariaService(IServiceScopeFactory scopeFactory, IConfiguration configuracao, ILogger<RotinaDiariaService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<RotinaDiariaService> _logger = logger;
        private readonly bool _habilitada = configuracao.GetValue("Rotinas:Habilitada", true);
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(Math.Max(configuracao.GetValue("Rotinas:IntervaloMinutos", 15), 1));
        private readonly TimeZoneInfo _fusoHorario = ObterFusoHorario();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_habilitada)
            {
                _logger.LogInformation("Rotinas em segundo plano desativadas por configuração.");
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var temporizador = new PeriodicTimer(_intervalo);

            do
            {
                try
                {
                    using var escopo = _scopeFactory.CreateScope();
                    var rotina = escopo.ServiceProvider.GetRequiredService<RotinaProcesso>();
                    var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _fusoHorario));

                    await rotina.ExecutarAsync(hoje);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Falha ao executar as rotinas em segundo plano.");
                }
            }
            while (await temporizador.WaitForNextTickAsync(stoppingToken));
        }

        private static TimeZoneInfo ObterFusoHorario()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "Brasília", "Brasília");
            }
        }
    }
}
