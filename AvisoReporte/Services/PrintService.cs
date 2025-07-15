using AvisoReporte.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class PrintService : IPrintService
    {
        private readonly string _rutaVisorPdf = string.IsNullOrEmpty(ConfigurationManager.AppSettings["RutaVisorPdf"])
            ? @"C:\Cinet\Profit\SumatraPDF.exe" : ConfigurationManager.AppSettings["RutaVisorPdf"]!;

        public async Task ImprimirReporte(List<string> rutaReportesPdf)
        {
            try
            {
                foreach (var rutaReporte in rutaReportesPdf)
                {
                    Log.Information($"Mandando a imprimir primer reporte: {rutaReporte}");

                    var psi = new ProcessStartInfo
                    {
                        FileName = _rutaVisorPdf,
                        Arguments = rutaReporte,
                        UseShellExecute = true
                    };
                    try
                    {
                        Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error en proceso de impresión: {ex.Message}");
                    }

                    Log.Information($"Se imprimó el primer reporte.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error en proceso de impresión: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}
