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

        public async Task<DataTable> ObtenerNumerosDeRetenciones(List<string> parametros, string? retencionFiltro = null)
        {
            var parametrosAdd = new List<string>(parametros);
            try
            {
                string consulta = @$"SELECT DISTINCT EGRC_NUMRET, EGRC_CONCEPTO FROM EGRESOS_C
                                    WHERE CBTEEG_CODIGO = ? and EGRE_NUMERO = ? and CBTEEGSUC_CODIGO = ? 
                                    AND EGRC_NUMRET != '0'";

                if (!string.IsNullOrEmpty(retencionFiltro))
                {
                    parametrosAdd.Add(retencionFiltro);
                    consulta = $@"SELECT * FROM EGRESOS_C c 
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

        public async Task<string> ObtenerImportes(List<string> parametros, string num_retencion, string tipoDeImporte)
        {
            var parametrosAdd = new List<string>(parametros);
            parametrosAdd.Add(num_retencion);
            try
            {
                string consulta = @$"SELECT EGRC_IMPORTE AS [BASE_IMPONIBLE] FROM EGRESOS_C
                                    WHERE CBTEEG_CODIGO = ? AND EGRE_NUMERO = ? AND CBTEEGSUC_CODIGO = ? AND EGRC_NUMRET = ? 
                                    AND EGRC_TIPO like '%{tipoDeImporte}'";
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
