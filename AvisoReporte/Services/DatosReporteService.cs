using System.Configuration;
using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class DatosReporteService : IDatosReporteService
    {
        private readonly DBConnect _conn;

        public DatosReporteService(DBConnect conn)
        {
            _conn = conn;
        }

        public async Task<OrdenDePagoModel> ObtenerOrdenDePago(string cod_Comprobante, string num_Comprobante, string cod_Sucursal, string cod_Proveedor, bool enviaEmail = true, bool esConsulta = false)
        {
            List<string> parametros = new List<string> { cod_Comprobante, num_Comprobante, cod_Sucursal };
            try
            {
                // Obtener datos de comprobantes: CBTEE_CODIGO, EGRE_NUMERO y SUC_CODIGO
                DataTable datosComprobante = await ObtenerDatosDeComprobante(parametros, esConsulta);
                if (datosComprobante.Rows.Count == 0) throw new Exception($"Comprobante {cod_Comprobante} - Número {num_Comprobante} - No se encontraron comprobantes.");

                // Obtener datos del proveedor.
                DataTable datosProveedor = await ObtenerDatosDeProveedor(cod_Proveedor);

                // Obtener datos de liquidaciones (CPRAS_CTACTE y COMPRAS)
                DataTable datosLiquidacion = await ObtenerDatosDeLiquidacion(parametros, cod_Proveedor);

                // Obtener datos de valores(VAL_MOVIMIENTOS Y VALORES TIPOS)
                DataTable datosValoresIng = await ObtenerDatosDeValores(parametros, true);
                DataTable datosValoresEgr = await ObtenerDatosDeValores(parametros, false);

                // Agregar los datos obtenidos al modelo de Orden de Pago
                OrdenDePagoModel ordenDePago = new OrdenDePagoModel();

                ordenDePago.CodigoComprobante = datosComprobante.Rows[0]["CBTEEG_CODIGO"].ToString().Trim();
                ordenDePago.NumeroComprobanteOPA = datosComprobante.Rows[0]["EGRE_NUMERO"].ToString().Trim();
                ordenDePago.CodigoSucursal = datosComprobante.Rows[0]["CBTEEGSUC_CODIGO"].ToString().Trim();
                ordenDePago.FechaOPA = Convert.ToDateTime(datosComprobante.Rows[0]["EGRE_FECHA"]);
                ordenDePago.CodigoProveedor = datosComprobante.Rows[0]["PRO_CODIGO"].ToString().Trim();
                ordenDePago.RazonSocialProveedor = datosProveedor.Rows[0]["PRO_LGLNOMBRE"].ToString().Trim();

                // Se guardan los emails en emailProveedores
                string emailProveedores = datosProveedor.Rows[0]["PRO_EMAIL"].ToString() == "" ? null : datosProveedor.Rows[0]["PRO_EMAIL"].ToString().Trim();

                /* Si el campo emailProveedores posee un ';', se considerá que hay más de un email en dicho campo
                 * Por lo que se separa los emails y se guardan en la lista del modelo de la orden de pago
                 */
                if (!string.IsNullOrEmpty(emailProveedores) && emailProveedores.Contains(';'))
                {
                    var emailArray = emailProveedores.Split(';');
                    foreach(var email in emailArray)
                    {
                        ordenDePago.EmailsProveedores.Add(email.Trim());
                    }
                }
                else if(!string.IsNullOrEmpty(emailProveedores))
                {
                    ordenDePago.EmailsProveedores.Add(emailProveedores);
                }

                if (!string.IsNullOrEmpty(emailProveedores) && !enviaEmail)
                {
                    ordenDePago.EmailsProveedores.Clear();
                }

                // Si se obtuvieron datos de liquidaciones, se agregan al modelo
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
                // Caso contrario, se buscan registros para liquidaciones en plan cuentas
                else
                {
                    datosLiquidacion = await ObtenerDatosDeLiquidacionPlanCodigo(parametros);
                    foreach (DataRow row in datosLiquidacion.Rows)
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
                // Movimientos de ingresos
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
                // Movimientos de egresos
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

                return ordenDePago;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener datos: {ex.Message}");
            }
        }
        
        public async Task<ClavesComprobantesModel> ObtenerClavesDeComprobante(string claves)
        {
            int index = -1;
            for (int i = 0; i < claves.Length; i++)
            {
                if (char.IsLetter(claves[i]))
                {
                    index = i;
                    break;
                }
            }
            int indexTotal = -1;
            for (int i = index; i < claves.Length; i++)
            {
                if (!char.IsLetter(claves[i]))
                {
                    indexTotal = i;
                    break;
                }
            }
            ClavesComprobantesModel clavesComprobantes = new ClavesComprobantesModel()
            {
                Cod_Comprobante = claves.Substring(index, indexTotal - index),
                Num_Comprobante = claves.Substring(indexTotal + 4),
                Cod_Sucursal = claves.Substring(indexTotal, 4),
                Cod_Proveedor = claves.Substring(0, index)
            };
            return clavesComprobantes;
        }

        public async Task<DataTable> ObtenerDatosDeComprobante(List<string> parametros, bool esConsulta = false)
        {
            /*
             * Si es consulta es false, se trabaja sobre la primer consulta. 
             * En la primer consulta hay un filtro que valida que la diferencia entre el número de egreso pasado
             * y el número de comprobante de cbte_egresos_n sea menor o igual a 100. Si la diferencia supera ese valor,
             * no traerá registros y se considerará que se está trabajando sobre la base de datos erronea y se pasará a
             * trabajar sobre la base de datos secundaría.
             */
            var consulta = $@"
                DECLARE @CbteegCod varchar(100) = ?;
                DECLARE @EgreNum varchar(100) = ?;
                DECLARE @SucCod varchar(100) = ?;

                SET DATEFORMAT DMY 
                SELECT CBTEEG_CODIGO, EGRE_NUMERO, CBTEEGSUC_CODIGO, EGRE_FECHA, PRO_CODIGO FROM EGRESOS_E 
                WHERE ETAL_CODIGO = '01' AND CBTEEG_CODIGO = @CbteegCod AND EGRE_NUMERO = @EgreNum AND CBTEEGSUC_CODIGO = @SucCod 
                AND ((select CBTEEGN_NUMERO from CBTE_EGRESOS_N where CBTEEG_CODIGO = @CbteegCod) - 
                    (select CONVERT(INT,EGRE_NUMERO) 
                        from EGRESOS_E 
                        where ETAL_CODIGO = '01' and CBTEEG_CODIGO = @CbteegCod and EGRE_NUMERO = @EgreNum and CBTEEGSUC_CODIGO = @SucCod)) <= 100";

            // Caso contrario, únicamente busca por las claves primarías.
            if (esConsulta)
            {
                consulta = @$"SELECT CBTEEG_CODIGO, EGRE_NUMERO, CBTEEGSUC_CODIGO, EGRE_FECHA, PRO_CODIGO FROM EGRESOS_E 
                                    WHERE ETAL_CODIGO = '01' AND CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? ;";
            }

            var resultado = await _conn.ObtenerRegistrosAsync(consulta, parametros);

            /* 
             * Si la query no trajo resultados, y no es una consulta. Hace la busqueda en una 
             * base de datos secundaria.
             * (a partir de este momento, el programara trabajará con esa base de datos secundaria).
             */
            if ((resultado is null || resultado.Rows.Count == 0) && !esConsulta)
            {
                _conn.UsaConfig = false;
                resultado = await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de comprobantes.");
            }

            return resultado;
        }

        public async Task<DataTable> ObtenerDatosDeProveedor(string codProveedor)
        {
            string consulta = $"SELECT PRO_LGLNOMBRE, PRO_CODIGO, PRO_EMAIL FROM PROVEEDORES WHERE PRO_CODIGO = ?";
            List<string> parametros = new List<string> { codProveedor };
            return await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de proveedor.");
        }

        public async Task<DataTable> ObtenerDatosDeLiquidacion(List<string> parametros, string codProveedor)
        {
            string consulta = @$"select CTAP_TIPOCBTE1, CTAP_NUMERO1, COME_FECHA, REPLACE(CONVERT(VARCHAR(100), cc.CTAP_IMPORTE),'-', '') AS CTAP_IMPORTE 
                                    from COMPRAS_E c right join CPRAS_CTACTE cc on c.COME_NUMERO = cc.CTAP_NUMERO1 and c.PRO_CODIGO = cc.PRO_CODIGO 
                                    WHERE CTAP_TIPOCBTE2= ? and CTAP_NUMERO2 = ? and ctaP_sucursal2 = ? 
                                    ORDER BY CTAP_NUMERO1, COME_FECHA;";

            return await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de liquidaciones.");
        }

        public async Task<DataTable> ObtenerDatosDeLiquidacionPlanCodigo(List<string> parametros)
        {
            string consulta = @$"select D.PLAN_CODIGO, P.PLAN_DESCRIPCION, D.EGRD_IMPORTE
                                    FROM EGRESOS_D D INNER JOIN PLAN_CUENTAS P ON P.PLAN_CODIGO = D.PLAN_CODIGO 
                                    where ETAL_CODIGO = '01' AND CBTEEG_CODIGO = ? 
                                    AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? AND EGRD_IMPUTACION='D';";
            return await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de liquidaciones.");
        }

        public async Task<DataTable> ObtenerDatosDeValores(List<string> parametros, bool inge_numero)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"SELECT v.VALM_FECHAVTO, v.VAL_CODIGO, vt.VAL_DESCRIPCION, valm_numero, VALM_IMPORTE FROM VAL_MOVIMIENTOS v ");
            sb.AppendLine($"INNER JOIN VALORES_TIPOS vt on vt.VAL_CODIGO = v.VAL_CODIGO ");

            if (inge_numero) sb.AppendLine($"WHERE ITAL_CODIGO = '01' AND CBTEIN_CODIGO = ? AND INGE_NUMERO = ? AND CBTEINSUC_CODIGO = ? ; ");
            else sb.AppendLine($"WHERE ETAL_CODIGO = '01' AND CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? ; ");

            string consulta = sb.ToString();
            return await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de valores.");
        }
    }
}