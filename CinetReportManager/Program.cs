using CinetReportManager.Interfaces;
using CinetReportManager.Models;
using CinetReportManager.Services;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

if (WindowsServiceHelpers.IsWindowsService())
{
    builder.Host.UseWindowsService();
    builder.WebHost.UseContentRoot(AppContext.BaseDirectory);
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowAnyOrigin();
        });
});

var port = builder.Configuration.GetValue<int>("Parametros:PuertoConfig");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<EmailModel>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddTransient<IOrdenDePagoService, OrdenDePagoService>();
builder.Services.AddTransient<IRetencionService, RetencionService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
