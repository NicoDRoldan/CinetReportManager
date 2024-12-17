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
        services.AddScoped<IAccesoDatosService, AccesoDatosService>();
        services.AddScoped<IMapeoDatosService, MapeoDatosService>();
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

List<string> impuestos = new List<string> { "RG830", "IIBB", "IVAM", "IIBBCABA", "IBMENDOZA", "IIBBSFE" };

if (args.Length == 1) /* Generación de reportes común */
{
    //Formado esperado: 009840OPA999900000001
    await app.AvisoReporte(args[0]);
}
/* Si la cantidad de parámetros es mayor a 1 se procede de otra forma */
else if (args.Length > 1 && impuestos.Contains(args[1].ToUpper())) /* Llamado a reimpresión de retenciones */
{
    //Formado esperado: 009840OPA999900000001 | RET+NOMBRERETENCION (o RET+* para todas las retenciones) 
    await app.ReimpresionRetenciones(args[0], args[1]);
}
else if (args.Length > 1 && (args[1].ToUpper() == "TRUE" || args[1].ToUpper() == "FALSE")) /* Llamado a reimpresión de comprobantes */
{
    //Formado esperado: 009840OPA999900000001 | True o False para envío de email
    await app.ReimpresionReporte(args[0], args[1]);
}
else
{
    Log.Error("No hay datos del comprobante.");
}