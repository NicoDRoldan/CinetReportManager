using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class DatosReportesRetencionesService : IDatosReportesRetencionesServices
    {
        private readonly IAccesoDatosService _accesoDatosService;
        private readonly IMapeoDatosService _mapeoDatosService;

        private readonly DBConnect _conn;

        public DatosReportesRetencionesService(DBConnect conn, IAccesoDatosService accesoDatosService, IMapeoDatosService mapeoDatosService)
        {
            _conn = conn;
            _accesoDatosService = accesoDatosService;
            _mapeoDatosService = mapeoDatosService;
        }

        public async Task<List<RetencionModel>> ObtenerRetencion(OrdenDePagoModel ordenDePago, string? retencionFiltro = null)
        {
            List<RetencionModel> lstRetenciones = new List<RetencionModel>();
            List<string> parametros = new List<string> { ordenDePago.CodigoComprobante, ordenDePago.NumeroComprobanteOPA, ordenDePago.CodigoSucursal } ;
            try
            {
                DataTable retenciones = await _accesoDatosService.ObtenerRetenciones(parametros, retencionFiltro);

                /* Validar que existan las retenciones */
                if ((retenciones is null || retenciones.Rows.Count == 0) && string.IsNullOrEmpty(retencionFiltro))
                {
                    throw new Exception("No existen retenciones para esta orden de pago.");

                }
                else if ((retenciones is null || retenciones.Rows.Count == 0) && !string.IsNullOrEmpty(retencionFiltro))
                {
                    throw new Exception($"No existe la retención {retencionFiltro} para la orden de pago {ordenDePago.NumeroComprobanteOPA}.");
                }

                foreach (DataRow retencion in retenciones.Rows)
                {
                    string cod_concepto = retencion["EGRC_CONCEPTO"].ToString();
                    string num_retencion = retencion["EGRC_NUMRET"].ToString();

                    DataTable datosRetencion = await _accesoDatosService.ObtenerDatosRetencion(cod_concepto);
                    DataTable datosProveedor = await _accesoDatosService.ObtenerDatosProveedor(ordenDePago.CodigoProveedor);

                    string importeImponible = await _accesoDatosService.ObtenerImportes(parametros, num_retencion, "P");
                    string importeRetenido = await _accesoDatosService.ObtenerImportes(parametros, num_retencion, "R");
                    string importeOrigina = await _accesoDatosService.ObtenerImporteOrigina(parametros);

                    RetencionModel ret = await _mapeoDatosService.MapeoRetenciones(cod_concepto, num_retencion, 
                        datosRetencion, importeImponible, importeRetenido, importeOrigina,
                        datosProveedor, ordenDePago, _conn.Base_Odbc, retencionFiltro);

                    lstRetenciones.Add(ret);
                }

                return lstRetenciones;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
