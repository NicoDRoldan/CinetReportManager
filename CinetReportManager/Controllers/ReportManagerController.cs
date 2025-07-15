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
            try
            {
                var respuestaGeneracionReporte = await _reportService.GenerarReporte(llamadoDto);

                return Ok(respuestaGeneracionReporte);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new GenerarReporteResponse
                {
                    Success = false,
                    Message = $"Error al generar el reporte: {ex.Message}"
                });
            }
        }

        [HttpPost("GenerarRetencion")]
        public async Task<IActionResult> GenerarRetencion([FromBody] List<RetencionModel> retenciones)
        {
            try
            {
                var respuestaGeneracionRet = await _reportService.GenerarRetenciones(retenciones);

                return Ok(respuestaGeneracionRet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenerarReporteResponse
                {
                    Success = false,
                    Message = $"Error al generar los reportes de retenciones: {ex.Message}"
                });
            }
        }
    }
}
