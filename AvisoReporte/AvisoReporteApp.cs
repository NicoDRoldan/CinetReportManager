using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.DTO;
using AvisoReporte.Models.Retenciones;
using Serilog;
using System;
using System.Configuration;

namespace AvisoReporte
{
    public class AvisoReporteApp
    {
        private readonly DBConnect _conn;
        private readonly IEnvioService _envioService;
        private readonly IDatosReporteService _datosReporteService;
        private readonly IDatosReportesRetencionesServices _datosReportesRetencionesServices;
        private readonly IPrintService _printService;

        public AvisoReporteApp (DBConnect conn, IEnvioService envioService ,IDatosReporteService datosReporteService, IDatosReportesRetencionesServices datosReportesRetencionesServices, IPrintService printService)
        {
            _conn = conn;
            _envioService = envioService;
            _datosReporteService = datosReporteService;
            _datosReportesRetencionesServices = datosReportesRetencionesServices;
            _printService = printService;
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
                OrdenDePagoModel opa = null;

                try
                {
                    opa = await _datosReporteService
                    .ObtenerOrdenDePago(ComprobanteDto.Cod_Comprobante, ComprobanteDto.Nro_Comprobante, ComprobanteDto.Nro_Sucursal, ComprobanteDto.Cod_Proveedor, enviaEmail);
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                }

                // Si la orden de pago es null, se intenta con la otra empresa
                if (opa is null)
                {
                    DBConnect.Empresa_Config = DBConnect.Empresa_Sec;
                    opa = await _datosReporteService
                    .ObtenerOrdenDePago(ComprobanteDto.Cod_Comprobante, ComprobanteDto.Nro_Comprobante, ComprobanteDto.Nro_Sucursal, ComprobanteDto.Cod_Proveedor, enviaEmail);
                }
                
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
                var respuestaApi = await _envioService.LlamadoApiCinetReportManager(llamadoDto);

                // Proceso de impresión:
                await _printService.LlamadoImpresion(respuestaApi.ArchivosGenerados);

                Log.Information($"*-----------------------------------------------------------*");
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
                // Obtener los datos de la Orden de pago
                OrdenDePagoModel opa = null;

                try
                {
                    opa = await _datosReporteService
                        .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, false, true);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                // Si la orden de pago es null, se intenta con la otra empresa
                if (opa is null)
                {
                    DBConnect.Empresa_Config = DBConnect.Empresa_Sec;
                    opa = await _datosReporteService
                    .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, false, true);
                }

                if (opa is null) throw new Exception("No se encontró una orden de pago");

                List<RetencionModel> retenciones = await _datosReportesRetencionesServices.ObtenerRetencion(opa, cod_retencion);

                var respuestaApi = await _envioService.LlamadoApiCinetReportManager(retenciones);

                // Proceso de impresión:
                await _printService.LlamadoImpresion(respuestaApi.ArchivosGenerados);
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