using AvisoReporte.Interfaces;
using AvisoReporte.Models.DTO;
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
        private readonly bool _imprimeOpas = (ConfigurationManager.AppSettings["ImprimirOpas"]?.ToUpper() ?? "") == "S";
        private readonly string _retencionesAImprimir = (ConfigurationManager.AppSettings["RetencionesAImprimir"]?.ToUpper() ?? "");

        private readonly string _rutaVisorPdf = string.IsNullOrEmpty(ConfigurationManager.AppSettings["RutaVisorPdf"])
            ? @"C:\Cinet\Profit\SumatraPDF.exe" : ConfigurationManager.AppSettings["RutaVisorPdf"]!;

        public async Task LlamadoImpresion(List<string> archivosGenerados)
        {
            try
            {
                List<string> reportesOpasGenerados = archivosGenerados.Where(r => !r.ToUpper().Contains("RETENCIONES")).ToList();
                List<string> reportesRetencionesGenerados = archivosGenerados.Where(r => r.ToUpper().Contains("RETENCIONES")).ToList();

                /* En esta lista se van a guardar aquellos archivos que se imprimirán. */
                List<string> archivosAImprimir = new();

                /* Si la Key del config indica que imprime OPAS, se agrega a la lista. */
                if (_imprimeOpas)
                    archivosAImprimir.AddRange(reportesOpasGenerados);

                /* Si la key RetencionesAImprimir no está vacío y contiene ; se van a separar las retenciones en un array */
                if (!string.IsNullOrEmpty(_retencionesAImprimir) && _retencionesAImprimir.Contains(';'))
                {
                    var retencionesArray = _retencionesAImprimir.Split(';');
                    foreach(var retencion in retencionesArray)
                    {
                        var nombreRet = string.Concat(retencion, "_");
                        var rutaArchivo = archivosGenerados.Where(r => r.Contains(nombreRet)).FirstOrDefault();

                        if(!string.IsNullOrEmpty(rutaArchivo))
                            archivosAImprimir.Add(rutaArchivo);
                    }
                }
                else if (!string.IsNullOrEmpty(_retencionesAImprimir))
                {
                    archivosAImprimir.Add(archivosGenerados.Where(r => r.Contains(string.Concat(_retencionesAImprimir.Trim(), "_"))).FirstOrDefault()!);
                }

                if (!archivosAImprimir.Any())
                    throw new Exception("No hay archivos para imprimir");
                
                await ImprimirReporte(archivosAImprimir);
            }
            catch(Exception ex)
            {
                throw new Exception($"Error en procesar los archivos generados para la impresión. {ex.Message}");
            }
        }

        public async Task ImprimirReporte(List<string> rutaReportesPdf)
        {
            try
            {
                foreach (var rutaReporte in rutaReportesPdf)
                {
                    Log.Information($"Mandando a imprimir reporte: {rutaReporte}");

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

                    Log.Information($"Se imprimó el reporte.");
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
