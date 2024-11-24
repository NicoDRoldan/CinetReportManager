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
    public class MapeoDatosService : IMapeoDatosService
    {
        public async Task<OrdenDePagoModel> MapeoOrdenDePago(DataTable datosComprobante, DataTable datosProveedor, DataTable datosLiquidacion, DataTable? datosLiquidacionPlanCodigo, DataTable datosValoresIng, DataTable datosValoresEgr, ClavesComprobantesModel claves, bool enviaEmail = true, bool esConsulta = false)
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

        public async Task<List<RetencionModel>> ObtenerRetencion(OrdenDePagoModel ordenDePago, string? retencionFiltro = null)
        {
            List<RetencionModel> retenciones = new List<RetencionModel>();

            try
            {
                return retenciones;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
