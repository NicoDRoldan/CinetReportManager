using CinetReportManager.Models;
using CinetReportManager.Models.Retenciones;

namespace CinetReportManager.Interfaces
{
    public interface IReportService
    {
        Task<MemoryStream> GenerarReporteOrdenDePago(OrdenDePagoModel ordenDePago);
        Task<MemoryStream> GenerarReporteRetencion(RetencionModel retencion);
    }
}
