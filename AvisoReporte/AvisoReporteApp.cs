using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.DTO;
using AvisoReporte.Models.Retenciones;
using AvisoReporte.Services;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvisoReporte
{
    public class AvisoReporteApp
    {
        private readonly DBConnect _conn;
        private readonly IEnvioService _envioService;
        private readonly IDatosReporteService _datosReporteService;
        private readonly IDatosReportesRetencionesServices _datosReportesRetencionesServices;

        public AvisoReporteApp (DBConnect conn, IEnvioService envioService ,IDatosReporteService datosReporteService, IDatosReportesRetencionesServices datosReportesRetencionesServices)
        {
            _conn = conn;
            _envioService = envioService;
            _datosReporteService = datosReporteService;
            _datosReportesRetencionesServices = datosReportesRetencionesServices;
        }

        public async Task AvisoReporte(string claves, bool enviaEmail = true)
        {
            try
            {
                /* 
                 * A traves del string "claves", obtengo:
                 * PRO_CODIGO.
                 * CBTEE_CODIGO.
                 * SUC_CODIGO.
                 * EGRE_NUMERO
                 */

                /* Se mapean las claves */
                await ComprobanteDto.ObtenerClavesDeComprobante(claves);

                /* Obtener registros desde la base de datos: */

                // Obtener los datos de la Orden de pago
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago(ComprobanteDto.Cod_Comprobante, ComprobanteDto.Nro_Comprobante, ComprobanteDto.Nro_Sucursal, ComprobanteDto.Cod_Proveedor, enviaEmail);

                // Si la orden de pago es null, se lanza excepción
                if (opa is null) throw new Exception("No se encontró una orden de pago");

                // Obtener los datos de las retenciones
                List<RetencionModel> retenciones = new();
                try
                {
                    retenciones = await _datosReportesRetencionesServices.ObtenerRetencion(opa);
                }
                catch(Exception ex)
                {
                    Log.Error($"Mensaje - {ComprobanteDto.Cod_Proveedor} - {ComprobanteDto.Nro_Comprobante} - {ex.Message}");
                }

                // Se guardan los datos en llamadoDto, que es el modelo que se enviará a CinetReportManager
                LlamadoDto llamadoDto = new LlamadoDto()
                {
                    BaseEmpresa = _conn.Base_Odbc ?? null,
                    OrdenDePago = opa,
                    Retenciones = retenciones is not null ? retenciones : null
                };

                // Enviar información a CinetReportManager:
                await _envioService.LlamadoApiCinetReportManager(llamadoDto);
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task AvisoRetencion(string cod_comprobante, string num_comprobante, string num_sucursal, string cod_proveedor ,string? cod_retencion = null)
        {
            try
            {
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, false, true);

                if (opa == null) throw new Exception("No se encontró una orden de pago");

                List<RetencionModel> retenciones = await _datosReportesRetencionesServices.ObtenerRetencion(opa, cod_retencion);

                await _envioService.LlamadoApiCinetReportManager(retenciones);
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task ReimpresionReporte(string claves, string envioEmailString)
        {
            /* Se transforma el string de envioEmail a bool */
            bool envioEmail = envioEmailString.ToUpper().Contains("T") ? true : false;

            try
            {
                /* Se llama al método para generar el reporte */
                await AvisoReporte(claves, envioEmail);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ReimpresionRetenciones(string claves, string codRetencion)
        {
            codRetencion = codRetencion.Contains("*") ? null! : codRetencion.Replace("RET", ""); /* Si codRetención es *, se considerará null. */
            
            try
            {
                /* Se mapean las claves */
                await ComprobanteDto.ObtenerClavesDeComprobante(claves);
                /* Se hace el llamado para regenerar las retenciones */
                await AvisoRetencion(ComprobanteDto.Cod_Comprobante, ComprobanteDto.Nro_Comprobante, ComprobanteDto.Nro_Sucursal, ComprobanteDto.Cod_Proveedor, codRetencion);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}