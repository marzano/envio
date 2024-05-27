using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Abstractions;
using NfImpacto.Envio.Domain.Model.Configuration;
using NfImpacto.Envio.Domain.Services;
using NfImpacto.Envio.Schedule;

namespace NfImpacto.Envio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly INfImpactoEnvioService _nfImpactoEnvioService;
        private readonly DatabaseOptions _databaseOptions;
        private readonly ServiceConfOptions _serviceConfOptions;
        private readonly TelemetryClient _telemetryClient;
        private ScheduleControl _daySchedule;
        private DateTime _dayNextRun;

        public Worker(ILogger<Worker> logger,
                        IOptions<ServiceConfOptions> serviceConfOptions,
                        IOptions<DatabaseOptions> databaseOptions,
                        TelemetryClient telemetryClient,
                        INfImpactoEnvioService nfImpactoEnvioService)
        {
            _logger = logger;
            _serviceConfOptions = serviceConfOptions.Value ?? throw new ArgumentNullException(nameof(serviceConfOptions));
            _databaseOptions = databaseOptions.Value ?? throw new ArgumentNullException(nameof(databaseOptions));
            _nfImpactoEnvioService = nfImpactoEnvioService;
            _telemetryClient = telemetryClient;

#if DEBUG
            //Processar();
            StartScheduler();
#else
            StartScheduler();
#endif
        }

        private void StartScheduler()
        {
            _daySchedule = new ScheduleControl(_serviceConfOptions.CrontabDayCommands);
            _dayNextRun = _daySchedule.GetNext();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("nfimpacto-envio"))
            {
                _telemetryClient.Context.InstrumentationKey = _serviceConfOptions.InstrumentationKey;
                _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} INICIANDO SERVICO");
                _telemetryClient.Flush();
            }

            _telemetryClient.Context.InstrumentationKey = _serviceConfOptions.InstrumentationKey;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} - Worker - Proxima execução será {_dayNextRun.ToString("dd/MM/yyyy HH:mm")}");
                _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} - Worker - Proxima execução será {_dayNextRun.ToString("dd/MM/yyyy HH:mm")}");
                var now = DateTime.Now;
                if (now > _dayNextRun)
                {
                    Processar();
                    _dayNextRun = _daySchedule.GetNext();
                }
                await Task.Delay(TimeSpan.FromMinutes(_serviceConfOptions.DelayRecurrenceInMinutes.Value), stoppingToken);
            }
        }

        private void Processar()
        {
            var msg = string.Format("NF Impacto Envio rodando as: {0}", DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(msg);
            }
            
            _telemetryClient.TrackEvent($"{_serviceConfOptions.CurrentAppName}-{_serviceConfOptions.Environment} " + msg);
            _nfImpactoEnvioService.Executar();
        }
    }
}