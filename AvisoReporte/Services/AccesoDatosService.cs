using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class AccesoDatosService : IAccesoDatosService
    {

        private readonly DBConnect _conn;

        public AccesoDatosService(DBConnect conn)
        {
            _conn = conn;
        }

        /* Orden de pago */

        public async Task<DataTable> ObtenerDatosDeComprobante(List<string> parametros, bool esConsulta = false)
        {
            var consulta = @$"SELECT CBTEEG_CODIGO, EGRE_NUMERO, CBTEEGSUC_CODIGO, EGRE_FECHA, PRO_CODIGO FROM EGRESOS_E 
                                    WHERE ETAL_CODIGO = '01' AND CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? ;";

            var resultado = await _conn.ObtenerRegistrosAsync(consulta, parametros);

            return resultado;
        }

        public async Task<DataTable> ObtenerDatosDeLiquidacion(List<string> parametros, string codProveedor)
        {
            string consulta = @$"select CTAP_TIPOCBTE1, CTAP_NUMERO1, COME_FECHA, REPLACE(CONVERT(VARCHAR(100), cc.CTAP_IMPORTE),'-', '') AS CTAP_IMPORTE 
                                    from COMPRAS_E c 
                                    right join CPRAS_CTACTE cc on c.COME_NUMERO = cc.CTAP_NUMERO1 and c.PRO_CODIGO = cc.PRO_CODIGO and c.CBTEE_CODIGO = cc.CTAP_TIPOCBTE1
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

        public async Task<DataTable> ObtenerDatosDeProveedor(string codProveedor)
        {
            List<string> parametros = new List<string> { codProveedor };
            string consulta = $"SELECT PRO_LGLNOMBRE, PRO_CODIGO, PRO_EMAIL FROM PROVEEDORES WHERE PRO_CODIGO = ?";

            return await _conn.ObtenerRegistrosAsync(consulta, parametros) ?? throw new Exception("No se obtuvieron datos de proveedor.");
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

        /* Retenciones */

        public async Task<DataTable> ObtenerRetenciones(List<string> parametros, string? retencionFiltro = null)
        {
            var parametrosAdd = new List<string>(parametros);
            try
            {
                string consulta = @$"SELECT DISTINCT EGRC_NUMRET, EGRC_CONCEPTO, SUBSTRING(EGRC_TIPO, 1, 1) AS EGRC_TIPO FROM EGRESOS_C
                                    WHERE CBTEEG_CODIGO = ? and EGRE_NUMERO = ? and CBTEEGSUC_CODIGO = ? 
                                    AND EGRC_NUMRET != '0' AND EGRC_CONCEPTO != '' ";

                if (!string.IsNullOrEmpty(retencionFiltro))
                {
                    parametrosAdd.Add(retencionFiltro);
                    consulta = $@"SELECT DISTINCT EGRC_NUMRET, EGRC_CONCEPTO, SUBSTRING(EGRC_TIPO, 1, 1) AS EGRC_TIPO FROM EGRESOS_C C 
                                    INNER JOIN RETEN_TABLA r ON r.RETEN_CODCONCEPTO = c.EGRC_CONCEPTO 
                                    WHERE CBTEEG_CODIGO = ? and EGRE_NUMERO = ? and CBTEEGSUC_CODIGO = ? 
                                    AND EGRC_NUMRET != '0' AND RETEN_CODIGO = ? ";
                }
                DataTable registros = await _conn.ObtenerRegistrosAsync(consulta, parametrosAdd);
                if (registros is null || registros.Rows.Count == 0)
                {
                    return null;
                }
                return registros;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> ObtenerDatosRetencion(string cod_concepto)
        {
            try
            {
                List<string> parametros = new List<string> { cod_concepto };
                string consulta = @$"SELECT RETEN_CODIGO, RETEN_CODCONCEPTO, RETEN_DESCCONCEPTO FROM RETEN_TABLA WHERE RETEN_CODCONCEPTO = ? ";
                DataTable registros = await _conn.ObtenerRegistrosAsync(consulta, parametros);
                return registros;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ObtenerCodigoConcepto(string pro_codigo, string cod_impuesto)
        {
            var parametros = new List<string> { pro_codigo, cod_impuesto };
            try
            {
                string consulta = "SELECT DISTINCT RETEN_CODCONCEPTO FROM PRORETEN WHERE PRO_CODIGO = ? AND RETEN_CODIGO = ?";
                string resultado = await _conn.ObtenerRegistroAsync(consulta, parametros);
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ObtenerImportes(List<string> parametros, string num_retencion, string tipoDeImporte, string cod_concepto)
        {
            var parametrosAdd = new List<string>(parametros);
            parametrosAdd.Add(num_retencion);
            parametrosAdd.Add(cod_concepto);
            try
            {
                string consulta = @$"SELECT TOP 1 EGRC_IMPORTE AS [BASE_IMPONIBLE] FROM EGRESOS_C
                                    WHERE CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? AND EGRC_NUMRET = ? AND EGRC_CONCEPTO = ? 
                                    AND EGRC_TIPO like '%{tipoDeImporte}' AND EGRC_IMPORTE != '0.00' ORDER BY EGRC_IMPORTE DESC";
                string resultado = await _conn.ObtenerRegistroAsync(consulta, parametrosAdd);
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ObtenerImporteOrigina(List<string> parametros)
        {
            try
            {
                string consulta = @$"SELECT TOP 1 EGRD_IMPORTE FROM EGRESOS_D
                                    WHERE CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? 
                                    ORDER BY EGRD_RENGLON DESC";
                string resultado = await _conn.ObtenerRegistroAsync(consulta, parametros);
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> ObtenerDatosProveedor(string cod_proveedor)
        {
            List<string> parametros = new List<string> { cod_proveedor };
            try
            {
                string consulta = @$"SELECT PRO_CODIGO, PRO_LGLNOMBRE, PRO_LGLCUIT, PRO_LGLDIRECCION, PRO_LGLLOCALIDAD, PROVIN_CODIGO FROM PROVEEDORES WHERE PRO_CODIGO = ? ";
                DataTable registros = await _conn.ObtenerRegistrosAsync(consulta, parametros);
                return registros;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
