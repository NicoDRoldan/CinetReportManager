using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IMapeoDatosService
    {
        Task<OrdenDePagoModel> MapeoOrdenDePago(
            DataTable datosComprobante, 
            DataTable datosProveedor,
            DataTable datosLiquidacion, 
            DataTable? datosLiquidacionPlanCodigo, 
            DataTable datosValoresIng, 
            DataTable datosValoresEgr,
            ClavesComprobantesModel clavesComprobantesModel,
            bool enviaEmail = true, 
            bool esConsulta = false
            );

        Task<List<RetencionModel>> ObtenerRetencion(OrdenDePagoModel ordenDePago, string? retencionFiltro = null);
    }
}
