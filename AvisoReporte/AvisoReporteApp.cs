using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models;
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
                ClavesComprobantesModel clavesComprobantes = await _datosReporteService.ObtenerClavesDeComprobante(claves);

                /* Obtener registros desde la base de datos: */

                // Obtener los datos de la Orden de pago
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago(clavesComprobantes.Cod_Comprobante, clavesComprobantes.Num_Comprobante, clavesComprobantes.Cod_Sucursal, clavesComprobantes.Cod_Proveedor, enviaEmail);

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
                    Log.Error($"Mensaje - {clavesComprobantes.Cod_Proveedor} - {clavesComprobantes.Num_Comprobante} - {ex.Message}");
                }

                // Se guardan los datos en llamadoDto, que es el modelo que se enviará a CinetReportManager
                LlamadoDto llamadoDto = new LlamadoDto()
                {
                    OrdenDePago = opa,
                    Retenciones = retenciones is not null ? retenciones : null
                };

                // Enviar información a CinetReportManager:
                var respuestaLlamado = await _envioService.LlamadoApiCinetReportManager(llamadoDto);

                // Generación de Log para aviso a usuario:
                Log.Information($"Se hizo el llamado correctamente: \n{respuestaLlamado}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task AvisoRegenerarReporte(string num_comprobante, string cod_proveedor, bool enviaEmail = true)
        {
            try
            {
                /* Obtener registros desde la base de datos: */

                // Obtener los datos de la Orden de pago
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, enviaEmail, true);

                // Si la orden de pago es null, se lanza excepción
                if (opa is null) throw new Exception("No se encontró una orden de pago");

                // Obtener los datos de las retenciones
                List<RetencionModel> retenciones = new();
                try
                {
                    retenciones = await _datosReportesRetencionesServices.ObtenerRetencion(opa);
                }
                catch (Exception ex)
                {
                    Log.Error($"Mensaje - {cod_proveedor} - {num_comprobante} - {ex.Message}");
                }

                // Se guardan los datos en llamadoDto, que es el modelo que se enviará a CinetReportManager
                LlamadoDto llamadoDto = new LlamadoDto()
                {
                    OrdenDePago = opa,
                    Retenciones = retenciones is not null ? retenciones : null
                };

                // Enviar información a CinetReportManager:
                var respuestaLlamado = await _envioService.LlamadoApiCinetReportManager(llamadoDto);

                // Generación de Log para aviso a usuario:
                Log.Information($"Se hizo el llamado correctamente: \n{respuestaLlamado}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task AvisoOrdenDePago(string num_comprobante, string cod_proveedor, bool enviaEmail = true)
        {
            try
            {
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, enviaEmail, true);

                if (opa == null) throw new Exception("No se encontró una orden de pago");

                LlamadoDto llamadoDto = new LlamadoDto()
                {
                    OrdenDePago = opa,
                    Retenciones = null
                };

                var respuestaLlamado = await _envioService.LlamadoApiCinetReportManager(llamadoDto);

                Log.Information($"Se hizo el llamado correctamente: \n{respuestaLlamado}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task AvisoRetencion(string num_comprobante, string cod_proveedor ,string? cod_retencion = null)
        {
            try
            {
                OrdenDePagoModel opa = await _datosReporteService
                    .ObtenerOrdenDePago("OPA", num_comprobante, "0121", cod_proveedor, false, true);

                if (opa == null) throw new Exception("No se encontró una orden de pago");

                List<RetencionModel> retenciones = await _datosReportesRetencionesServices.ObtenerRetencion(opa, cod_retencion);

                var respuestaLlamado = await _envioService.LlamadoApiCinetReportManager(retenciones);

                Log.Information($"Se hizo el llamado correctamente: \n{respuestaLlamado}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task SolicitudRegenerarReporte()
        {
            string opcion;

            while (true)
            {
                Console.Clear();

                Console.WriteLine("REGENERAR REPORTE:\n");
                Console.WriteLine("Seleccionar opción:");
                Console.WriteLine("1) Regenerar Orden de Pago y Retenciones (Con envío de email).");
                Console.WriteLine("2) Regenerar Orden de Pago y Retenciones (Sin envío de email).");
                Console.WriteLine("3) Regenerar Orden de Pago.");
                Console.WriteLine("4) Regenerar Retención.");
                Console.WriteLine("5) Salir.");

                opcion = Console.ReadLine()!;
                bool opcionValida = false;

                while (Convert.ToInt32(opcion) < 1 || Convert.ToInt32(opcion) > 5)
                {
                    Console.WriteLine("Elegir una opción válida");
                    opcion = Console.ReadLine()!;
                }

                if(opcion == "5")
                {
                    break;
                }

                try
                {
                    await ReporteARegenerar(opcion);

                    Console.WriteLine("Se regeneró el reporte.");
                    Console.ReadKey();

                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al regenerar el reporte.");
                    Console.ReadKey();

                    Log.Error($"Error: {ex.Message}");
                }
            }
        }

        public async Task ReporteARegenerar(string opcion)
        {
            string nro_comprobante = "";
            string cod_proveedor = "";
            string cod_reten = "";

            while (string.IsNullOrEmpty(nro_comprobante) || string.IsNullOrEmpty(cod_proveedor))
            {
                Console.WriteLine("Indicar el número de orden de pago: ");
                nro_comprobante = Console.ReadLine()!;
                Console.WriteLine("Indicar el código de proveedor: ");
                cod_proveedor = Console.ReadLine()!;
            }

            if(opcion == "4")
            {
                bool valido = false;
                string opcionRet = "";

                Console.WriteLine("Elegir el código de retención: ");
                Console.WriteLine("1) IIBB");
                Console.WriteLine("2) IIBBCABA");
                Console.WriteLine("3) IBMENDOZA");
                Console.WriteLine("4) RG830");
                Console.WriteLine("5) IVAM");
                Console.WriteLine("6) Regenrar todas las retenciones asociadas");

                opcionRet = Console.ReadLine();

                while (!valido)
                {
                    switch (opcionRet)
                    {
                        case "1":
                            cod_reten = "IIBB";
                            valido = true;
                            break;
                        case "2":
                            cod_reten = "IIBBCABA";
                            valido = true;
                            break;
                        case "3":
                            cod_reten = "IBMENDOZA";
                            valido = true;
                            break;
                        case "4":
                            cod_reten = "RG830";
                            valido = true;
                            break;
                        case "5":
                            cod_reten = "IVAM";
                            valido = true;
                            break;
                        case "6":
                            cod_reten = null;
                            valido = true;
                            break;
                        default:
                            Console.WriteLine("Seleccionar una opción válida.");
                            opcionRet = Console.ReadLine();
                            break;
                    }
                }
            }
            bool reintento = true;
            while (reintento)
            {
                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Console.WriteLine("Se generarán nuevamente los reportes de Orden de Pago y Retenciones.");
                            reintento = false;
                            await AvisoRegenerarReporte(nro_comprobante, cod_proveedor, true);
                            break;
                        case "2":
                            Console.WriteLine("Se generarán nuevamente los reportes de Orden de Pago y Retenciones.");
                            reintento = false;
                            await AvisoRegenerarReporte(nro_comprobante, cod_proveedor, false);
                            break;
                        case "3":
                            Console.WriteLine("Se generarán nuevamente el reporte de Orden de Pago.");
                            reintento = false;
                            await AvisoOrdenDePago(nro_comprobante, cod_proveedor, false);
                            break;
                        case "4":
                            Console.WriteLine("Se generará nuevamente el/los reporte/s de Retenciones.");
                            reintento = false;
                            await AvisoRetencion(nro_comprobante, cod_proveedor, cod_reten);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.ToString().ToLower().Contains("no existe"))
                    {
                        Console.WriteLine($"{ex.Message}");
                        Console.ReadKey();
                        throw new Exception();
                    }
                    else
                    {
                        Console.WriteLine($"Ocurrió un error.");
                        Console.WriteLine($"¿Volver a intentar? (S/N)");
                        var respuesta = Console.ReadLine().Trim().ToUpper();
                        if (respuesta == "S") reintento = true;
                        else throw new Exception();
                    }
                }
            }
        }

    }
}