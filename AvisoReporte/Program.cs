using AvisoReporte;
using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddScoped<AvisoReporteApp>();
        services.AddScoped<DBConnect>();
        services.AddScoped<IDatosReporteService, DatosReporteService>();
        services.AddScoped<IDatosReportesRetencionesServices, DatosReportesRetencionesService>();
        services.AddScoped<IEnvioService, EnvioService>();
    })
    .Build();

Log.Logger = new LoggerConfiguration()
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
                    .WriteTo.File("LogsError/AvisoReporteError_.txt", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information)
                    .WriteTo.File("Logs/AvisoReporteLog_.txt", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning)
                    .WriteTo.File("LogsWarning/AvisoReporteLog_.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

var app = host.Services.GetRequiredService<AvisoReporteApp>();

if (args.Length > 0)
{
    //Formado esperado: 009840OPA999900000001
    await app.AvisoReporte(args[0]);
}
else if(args.Length == 0)
{
    await app.SolicitudRegenerarReporte();
}
else
{
    Log.Error("No hay datos del comprobante.");
}