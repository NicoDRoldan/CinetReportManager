using CinetReportManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas.Draw;
using CinetReportManager.Interfaces;
using CinetReportManager.Services;
using iText.Pdfua.Checkers.Utils;
using CinetReportManager.Models.Retenciones;
using CinetReportManager.Models.DTO;
using System.Text;

namespace CinetReportManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportManagerController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;

        public ReportManagerController(IReportService reportService, IEmailService emailService)
        {
            _reportService = reportService;
            _emailService = emailService;
        }

        [HttpPost("GenerarReporte")]
        public async Task<IActionResult> GenerarReporte([FromBody] LlamadoDto llamadoDto)
        {
            List<MemoryStream> streams = new List<MemoryStream>();
            Dictionary<string, Stream> streamsDictionary = new Dictionary<string, Stream>();

            StringBuilder sb = new StringBuilder();

            try
            {
                streamsDictionary[$"{llamadoDto.OrdenDePago.CodigoComprobante}_{llamadoDto.OrdenDePago.NumeroComprobanteOPA}.pdf"] = await _reportService.GenerarReporteOrdenDePago(llamadoDto.OrdenDePago, llamadoDto.BaseEmpresa);

                if (!streamsDictionary.Any()) throw new Exception("Error al generar el reporte de la orden de pago.");

                sb.AppendLine($"Se generó el reporte del comprobante {llamadoDto.OrdenDePago.CodigoComprobante} número {llamadoDto.OrdenDePago.NumeroComprobanteOPA}.");

                if (llamadoDto.Retenciones is not null && llamadoDto.Retenciones.Any())
                {
                    try
                    {
                        foreach (var retencion in llamadoDto.Retenciones)
                        {
                            try
                            {
                                string nombreArchivo = $"{retencion.CodigoRetencion}_{retencion.NumeroRetencion}_{retencion.RetencionPracticada.TipoComprobante}_{retencion.RetencionPracticada.NumeroComprobante}.pdf";
                                streamsDictionary[nombreArchivo] = await _reportService.GenerarReporteRetencion(retencion);
                                sb.AppendLine($"La generación de la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion} fue correcta");
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"Error en la generación de al retención {retencion.CodigoRetencion}_{retencion.NumeroRetencion}_{retencion.RetencionPracticada.TipoComprobante}_{retencion.RetencionPracticada.NumeroComprobante}. Validar. {ex.Message}");
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        sb.AppendLine($"Error en la generación de retenciones. Validar. {ex.Message}");
                    }
                }
                try
                {
                    //await _emailService.EnviarEmailAProveedor(llamadoDto.OrdenDePago.EmailsProveedores, llamadoDto.OrdenDePago.NumeroComprobanteOPA, streamsDictionary);
                    sb.AppendLine("El envío del email fue correcto.");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Se generó el reporte, sin embargo el envío del email falló por: {ex.Message}");
                }

                return Ok(new
                {
                    Message = sb.ToString()
                });
            }
            catch(Exception ex)
            {
                return BadRequest(@$"{ex.Message}");
            }
            finally
            {
                foreach(var stream in streams)
                {
                    stream.Dispose();
                }
            }
        }

        [HttpPost("GenerarRetencion")]
        public async Task<IActionResult> GenerarRetencion([FromBody] List<RetencionModel> retenciones)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            try
            {
                foreach(var retencion in retenciones)
                {
                    try
                    {
                        await _reportService.GenerarReporteRetencion(retencion);
                        sb.AppendLine($"Se generó la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion}.");
                        i++;
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error al generar la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion} - {ex.Message}.");
                    }
                }
                string msg = sb.ToString();
                if (i == 0)
                {                    
                    throw new Exception($"No se generaron las retenciones: {msg}");
                }

                return Ok(new
                {
                    Message = sb.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(@$"{ex.Message}");
            }
        }

    }
}
