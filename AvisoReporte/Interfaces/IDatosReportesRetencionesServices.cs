using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IDatosReportesRetencionesServices
    {
        Task<List<RetencionModel>> ObtenerRetencion(OrdenDePagoModel ordenDePago, string? retencionFiltro = null);
    }
}
