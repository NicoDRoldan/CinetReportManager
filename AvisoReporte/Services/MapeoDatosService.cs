using AvisoReporte.Interfaces;
using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class MapeoDatosService : IMapeoDatosService
    {
        private string _Denominacion;
        private string _DireccionAgente;
        private string _IvaAgente;
        private string _CuitAgente;

        public async Task<OrdenDePagoModel> MapeoOrdenDePago(DataTable datosComprobante, DataTable datosProveedor, DataTable datosLiquidacion, DataTable? datosLiquidacionPlanCodigo, DataTable datosValoresIng, 
            DataTable datosValoresEgr, ClavesComprobantesModel claves, bool enviaEmail = true, bool esConsulta = false)
        {
            try
            {
                if (datosComprobante.Rows.Count == 0)
                    throw new Exception($"Comprobante {claves.Cod_Comprobante} - Número {claves.Num_Comprobante} - No se encontraron comprobantes.");

                /* Agregar los datos obtenidos al modelo de Orden de Pago */

                OrdenDePagoModel ordenDePago = new OrdenDePagoModel();

                ordenDePago.CodigoComprobante = datosComprobante.Rows[0]["CBTEEG_CODIGO"].ToString().Trim();
                ordenDePago.NumeroComprobanteOPA = datosComprobante.Rows[0]["EGRE_NUMERO"].ToString().Trim();
                ordenDePago.CodigoSucursal = datosComprobante.Rows[0]["CBTEEGSUC_CODIGO"].ToString().Trim();
                ordenDePago.FechaOPA = Convert.ToDateTime(datosComprobante.Rows[0]["EGRE_FECHA"]);
                ordenDePago.CodigoProveedor = datosComprobante.Rows[0]["PRO_CODIGO"].ToString().Trim();
                ordenDePago.RazonSocialProveedor = datosProveedor.Rows[0]["PRO_LGLNOMBRE"].ToString().Trim();

                /* Mapeo de Emails */

                string emailProveedores = datosProveedor.Rows[0]["PRO_EMAIL"].ToString() == "" ? null : datosProveedor.Rows[0]["PRO_EMAIL"].ToString().Trim();

                /* 
                 * Si el campo emailProveedores posee un ';', se considerá que hay más de un email en dicho campo
                 * Por lo que se separa los emails y se guardan en la lista del modelo de la orden de pago
                 */
                if (!string.IsNullOrEmpty(emailProveedores) && emailProveedores.Contains(';'))
                {
                    var emailArray = emailProveedores.Split(';');
                    foreach (var email in emailArray)
                    {
                        ordenDePago.EmailsProveedores.Add(email.Trim());
                    }
                }
                else if (!string.IsNullOrEmpty(emailProveedores))
                {
                    ordenDePago.EmailsProveedores.Add(emailProveedores);
                }

                if (!string.IsNullOrEmpty(emailProveedores) && !enviaEmail)
                {
                    ordenDePago.EmailsProveedores.Clear();
                }

                /* Mapeo de Liquidaciones */

                /* Si se obtuvieron datos de liquidaciones, se agregan al modelo */
                if (datosLiquidacion.Rows.Count > 0)
                {
                    foreach (DataRow row in datosLiquidacion.Rows)
                    {
                        var importeString = row["CTAP_IMPORTE"].ToString();
                        var importeCorregido = importeString.Replace(".", ",");
                        ordenDePago.Liquidaciones.Add(new LiquidacionModel()
                        {
                            CodigoComprobante = row["CTAP_TIPOCBTE1"].ToString().Trim(),
                            NumeroComprobanteFAC = row["CTAP_NUMERO1"].ToString().Trim(),
                            FechaFactura = row["CTAP_TIPOCBTE1"].ToString().Trim().ToUpper() == "AN" ? ordenDePago.FechaOPA : Convert.ToDateTime(row["COME_FECHA"]),
                            Saldo = 0,
                            ImportePagado = decimal.TryParse(importeCorregido, out var importe) ? importe : 0
                        });
                    }
                }
                /* Caso contrario, se buscan registros para liquidaciones en plan cuentas */
                else
                {
                    foreach (DataRow row in datosLiquidacionPlanCodigo.Rows)
                    {
                        var importeString = row["EGRD_IMPORTE"].ToString();
                        var importeCorregido = importeString.Replace(".", ",");
                        ordenDePago.Liquidaciones.Add(new LiquidacionModel()
                        {
                            CodigoComprobante = row["PLAN_CODIGO"].ToString().Trim(),
                            NumeroComprobanteFAC = row["PLAN_DESCRIPCION"].ToString().Trim(),
                            FechaFactura = Convert.ToDateTime(datosComprobante.Rows[0]["EGRE_FECHA"]),
                            Saldo = 0,
                            ImportePagado = decimal.TryParse(importeCorregido, out var importe) ? importe : 0
                        });
                    }
                }

                /* Mapeo de Ingresos y Egresos */

                /* Movimietos de Ingresos */
                foreach (DataRow row in datosValoresIng.Rows)
                {
                    ordenDePago.ValoresIng.Add(new ValorModel()
                    {
                        FechaValor = Convert.ToDateTime(row["VALM_FECHAVTO"]),
                        Descripcion = $"{row["VAL_DESCRIPCION"].ToString()}",
                        NumMovimiento = $"{row["valm_numero"].ToString()}",
                        Importe = Convert.ToDecimal(row["VALM_IMPORTE"])
                    });
                }
                /* Movimientos de egresos */
                foreach (DataRow row in datosValoresEgr.Rows)
                {
                    ordenDePago.ValoresEgr.Add(new ValorModel()
                    {
                        FechaValor = Convert.ToDateTime(row["VALM_FECHAVTO"]),
                        Descripcion = $"{row["VAL_DESCRIPCION"].ToString()}",
                        NumMovimiento = $"{row["valm_numero"].ToString()}",
                        Importe = Convert.ToDecimal(row["VALM_IMPORTE"])
                    });
                }

                /* Se retorna la orden de pago */
                return ordenDePago;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<RetencionModel> MapeoRetenciones(string cod_concepto, string num_retencion, DataTable datosRetencion, string importeImponible, string importeRetenido, string importeOrigina, DataTable datosProveedor,
            OrdenDePagoModel ordenDePago, string baseEmpresa, string? retencionFiltro = null)
        {
            try
            {
                /* Agregar los datos obtenidos a los modelos correspondientes */
                string cod_retencion = datosRetencion.Rows[0]["RETEN_CODIGO"].ToString().Trim();

                // Calcular importes
                decimal importeImponibleDecimal = Convert.ToDecimal(importeImponible);
                decimal importeRetenidoDecimal = Convert.ToDecimal(importeRetenido);
                decimal importeOriginaDecimal = Convert.ToDecimal(importeOrigina);
                string porcentajeCalculadoString = ((importeRetenidoDecimal / importeImponibleDecimal) * 100).ToString("0.00");
                decimal porcentajeCalculado = Convert.ToDecimal(porcentajeCalculadoString);

                await ValidarDatosAgentesDeRetencion(baseEmpresa);

                RetencionModel retencionModel = new RetencionModel
                {
                    NumeroRetencion = num_retencion,
                    CodigoRetencion = cod_retencion,
                    CodigoConcepto = cod_concepto.Trim(),
                    Fecha = ordenDePago.FechaOPA,
                    IB = "901-039363-6", // Dato hardcodeado
                    AgenteRetencion = new AgenteRetencionModel()
                    {
                        Denominacion = _Denominacion,
                        DireccionAgente = _DireccionAgente,
                        IvaAgente = _IvaAgente,
                        CuitAgente = _CuitAgente
                    },
                    SujetoRetenido = new SujetoRetenidoModel()
                    {
                        RazonSocialSujeto = datosProveedor.Rows[0]["PRO_LGLNOMBRE"].ToString(),
                        CuitSujeto = datosProveedor.Rows[0]["PRO_LGLCUIT"].ToString(),
                        DireccionRetenido = @$"{datosProveedor.Rows[0]["PRO_LGLDIRECCION"].ToString().Trim()} {datosProveedor.Rows[0]["PRO_LGLLOCALIDAD"].ToString().Trim()} {datosProveedor.Rows[0]["PROVIN_CODIGO"].ToString().Trim()}",
                        CodigoProveedor = datosProveedor.Rows[0]["PRO_CODIGO"].ToString(),
                    },
                    RetencionPracticada = new RetencionPracticadaModel()
                    {
                        TipoImpuesto = ValidarTipoImpuesto(cod_retencion),
                        TipoComprobante = ordenDePago.CodigoComprobante,
                        NumeroComprobante = ordenDePago.NumeroComprobanteOPA,
                        DescripcionReten = datosRetencion.Rows[0]["RETEN_DESCCONCEPTO"].ToString().Trim(),
                        ImporteOriginaReten = importeOriginaDecimal
                    },
                    BaseImponible = importeImponibleDecimal,
                    Porcentaje = porcentajeCalculado,
                    ImporteRetencion = importeRetenidoDecimal,
                    Firma = baseEmpresa == "MOSTAZA_ERP" ? "MOSTAZA Y PAN S.A. APODERADO" : null
                };

                return retencionModel;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string ValidarTipoImpuesto(string cod_retencion)
        {
            string tipo_impuesto = "";
            switch (cod_retencion)
            {
                case "IIBB":
                    tipo_impuesto = "Ingresos Brutos de la Prov. de Bs. As.";
                    break;
                case "IIBBCABA":
                    tipo_impuesto = "Ingresos Brutos Capital Federal";
                    break;
                case "IBMENDOZA":
                    tipo_impuesto = "Ingresos Brutos Mendoza";
                    break;
                case "IIBBSFE":
                    tipo_impuesto = "Ingresos Brutos Santa Fe";
                    break;
                case "IVAM":
                    tipo_impuesto = "Impuesto al Valor Agregado";
                    break;
                case "RG830":
                    tipo_impuesto = "Impuesto a las Ganancias";
                    break;
                case "RSUSS":
                    tipo_impuesto = "Aportes Seguridad Social (SUSS)";
                    break;
                default:
                    tipo_impuesto = cod_retencion;
                    break;
            }
            return tipo_impuesto;
        }

        public async Task ValidarDatosAgentesDeRetencion(string baseEmpresa)
        {
            switch (baseEmpresa)
            {
                case "GADA_GROUP_ERP":
                    _Denominacion = "GADA GROUP"; // Dato hardcodeado
                    _DireccionAgente = "COSSETTINI,OLGA 152 Piso:8 Dpto:7, CIUDAD AUTONOMA BUENOS AIRES"; // Dato hardcodeado
                    _IvaAgente = "Responsable Inscripto"; // Dato hardcodeado
                    _CuitAgente = "30-71583994-2"; // Dato hardcodeado
                    break;
                case "GALDEANO_ERP":
                    _Denominacion = "GALDEANO ALVARADO CHRISTIAN DANIEL"; // Dato hardcodeado
                    _DireccionAgente = "COSSETTINI,OLGA 152 Piso:8 Dpto:7, CIUDAD AUTONOMA BUENOS AIRES"; // Dato hardcodeado
                    _IvaAgente = "Responsable Inscripto"; // Dato hardcodeado
                    _CuitAgente = "20-23426454-1"; // Dato hardcodeado
                    break;
                case "MOSTAZA_ERP":
                    _Denominacion = "Mostaza y Pan S.A"; // Dato hardcodeado
                    _DireccionAgente = "Au. Bs.As. - La Plata Km.9 Local 1003, Avellaneda - Pcía de Buenos Aires."; // Dato hardcodeado
                    _IvaAgente = "Responsable Inscripto"; // Dato hardcodeado
                    _CuitAgente = "33-70701313-9"; // Dato hardcodeado
                    break;
                default:
                    _Denominacion = ConfigurationManager.AppSettings["Agente.Default.Denominacion"] ?? "-";
                    _DireccionAgente = ConfigurationManager.AppSettings["Agente.Default.Direccion"] ?? "-";
                    _IvaAgente = ConfigurationManager.AppSettings["Agente.Default.Iva"] ?? "-";
                    _CuitAgente = ConfigurationManager.AppSettings["Agente.Default.Cuit"] ?? "-";
                    break;
            }
        }
    }
}
