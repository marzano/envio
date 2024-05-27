using NfImpacto.Envio;
using Microsoft.Extensions.Hosting;
using NfImpacto.Envio.Configurations.Factories;
using NfImpacto.Envio.Domain.Model.Configuration;
using NfImpacto.Envio.Domain.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Envio NF Impacto";
});

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.Database));

builder.Services.Configure<ArquivoExcelOptions>(
    builder.Configuration.GetSection(ArquivoExcelOptions.ArquivoExcel));

builder.Services.Configure<ServiceConfOptions>(
    builder.Configuration.GetSection(ServiceConfOptions.ServiceConf));


builder.Services.AddSingleton<IDatabaseFactory, DatabaseFactory>();

builder.Services.AddTransient<ISqlService, SqlService>();
builder.Services.AddTransient<IArquivoExcelService, ArquivoExcelService>();
builder.Services.AddTransient<INfImpactoEnvioService, NfImpactoEnvioService>();

builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();
