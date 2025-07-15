using CinetReportManager.Models;
using CinetReportManager.Models.DTO;
using CinetReportManager.Models.Retenciones;

namespace CinetReportManager.Interfaces
{
    public interface IReportService
    {
        Task<MemoryStream> GenerarReporteOrdenDePago(OrdenDePagoModel ordenDePago, string? baseEmpresa = null);
        Task<MemoryStream> GenerarReporteRetencion(RetencionModel retencion);
        Task<GenerarReporteResponse> GenerarReporte(LlamadoDto llamadoDto);
        Task<GenerarReporteResponse> GenerarRetenciones(List<RetencionModel> retenciones);
    }
}
