using AvisoReporte.Data;
using AvisoReporte.Interfaces;
using AvisoReporte.Models;
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
        private readonly DBConnect _conn;

        public DatosReportesRetencionesService(DBConnect conn)
        {
            _conn = conn;
        }

        public async Task<List<RetencionModel>> ObtenerRetencion(OrdenDePagoModel ordenDePago, string? retencionFiltro = null)
        {
            List<RetencionModel> lstRetenciones = new List<RetencionModel>();
            List<string> parametros = new List<string> { ordenDePago.CodigoComprobante, ordenDePago.NumeroComprobanteOPA, ordenDePago.CodigoSucursal } ;
            try
            {
                // Obtener los números de retenciones de la orden de pago.
                DataTable nrosRetenciones = await ObtenerNumerosDeRetenciones(parametros, retencionFiltro);

                if ((nrosRetenciones is null || nrosRetenciones.Rows.Count == 0) && string.IsNullOrEmpty(retencionFiltro))
                {
                    throw new Exception("No existen retenciones para esta orden de pago.");
                    
                }
                else if((nrosRetenciones is null || nrosRetenciones.Rows.Count == 0) && !string.IsNullOrEmpty(retencionFiltro))
                {
                    throw new Exception($"No existe la retención {retencionFiltro} para la orden de pago {ordenDePago.NumeroComprobanteOPA}.");
                }

                // Recorrer los números de retenciones obtenidos
                foreach(DataRow nroRetencion in nrosRetenciones.Rows)
                {
                    string cod_concepto = nroRetencion["EGRC_CONCEPTO"].ToString();
                    string num_retencion = nroRetencion["EGRC_NUMRET"].ToString();

                    DataTable datosRetencion = await ObtenerDatosRetencion(cod_concepto);
                    string cod_retencion = datosRetencion.Rows[0]["RETEN_CODIGO"].ToString().Trim();

                    // Obtener importes
                    string importeImponibleString = await ObtenerImportes(parametros, num_retencion, "P");
                    string importeRetenidoString = await ObtenerImportes(parametros, num_retencion, "R");
                    string ImporteOriginaString = await ObtenerImporteOrigina(parametros);

                    // Calcular importes
                    decimal importeImponible = Convert.ToDecimal(importeImponibleString);
                    decimal importeRetenido = Convert.ToDecimal(importeRetenidoString);
                    decimal importeOrigina = Convert.ToDecimal(ImporteOriginaString);
                    string porcentajeCalculadoString = ((importeRetenido / importeImponible) * 100).ToString("0.00");
                    decimal porcentajeCalculado = Convert.ToDecimal(porcentajeCalculadoString);

                    DataTable datosProveedor = await ObtenerDatosProveedor(ordenDePago.CodigoProveedor);

                    // Por cada número de retención, se creará un nuevo modelo que posteriormente se agregará a una lista de retenciones
                    RetencionModel retencion = new RetencionModel()
                    {
                        NumeroRetencion = num_retencion,
                        CodigoRetencion = cod_retencion,
                        Fecha = ordenDePago.FechaOPA,
                        IB = "901-039363-6", // Dato hardcodeado
                        AgenteRetencion = new AgenteRetencionModel()
                        {
                            Denominacion = "Mostaza y Pan S.A", // Dato hardcodeado
                            DireccionAgente = "Au. Bs.As. - La Plata Km.9 Local 1003, Avellaneda - Pcía de Buenos Aires.", // Dato hardcodeado
                            IvaAgente = "Responsable Inscripto", // Dato hardcodeado
                            CuitAgente = "33-70701313-9" // Dato hardcodeado
                        },
                        SujetoRetenido = new SujetoRetenidoModel()
                        {
                            RazonSocialSujeto = datosProveedor.Rows[0]["PRO_LGLNOMBRE"].ToString(),
                            CuitSujeto = datosProveedor.Rows[0]["PRO_LGLCUIT"].ToString(),
                            DireccionRetenido = @$"{datosProveedor.Rows[0]["PRO_LGLDIRECCION"].ToString().Trim()} {datosProveedor.Rows[0]["PRO_LGLLOCALIDAD"].ToString().Trim()} {datosProveedor.Rows[0]["PROVIN_CODIGO"].ToString().Trim()}"
                        },
                        RetencionPracticada = new RetencionPracticadaModel()
                        {
                            TipoImpuesto = ValidarTipoImpuesto(cod_retencion),
                            TipoComprobante = ordenDePago.CodigoComprobante,
                            NumeroComprobante = ordenDePago.NumeroComprobanteOPA,
                            DescripcionReten = datosRetencion.Rows[0]["RETEN_DESCCONCEPTO"].ToString().Trim(),
                            ImporteOriginaReten = importeOrigina
                        },
                        BaseImponible = importeImponible,
                        Porcentaje = porcentajeCalculado,
                        ImporteRetencion = importeRetenido,
                        Firma = "MOSTAZA Y PAN S.A. APODERADO" // Dato hardcodeado
                    };
                    lstRetenciones.Add(retencion);
                }
                return lstRetenciones;
            }
            catch (Exception ex)
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
                case "IVAM":
                    tipo_impuesto = "Impuesto al Valor Agregado";
                    break;
                case "RG830":
                    tipo_impuesto = "Impuesto a las Ganancias";
                    break;
            }
            return tipo_impuesto;
        }

        public async Task<DataTable> ObtenerNumerosDeRetenciones(List<string> parametros, string? retencionFiltro = null)
        {
            var parametrosAdd = new List<string>(parametros);
            try
            {
                string consulta = @$"SELECT DISTINCT EGRC_NUMRET, EGRC_CONCEPTO FROM EGRESOS_C
                                    WHERE CBTEEG_CODIGO = ? and EGRE_NUMERO = ? and CBTEEGSUC_CODIGO = ? 
                                    AND EGRC_NUMRET != '0'";

                if(!string.IsNullOrEmpty(retencionFiltro))
                {
                    parametrosAdd.Add(retencionFiltro);
                    consulta = $@"SELECT * FROM EGRESOS_C c 
                                    INNER JOIN RETEN_TABLA r ON r.RETEN_CODCONCEPTO = c.EGRC_CONCEPTO 
                                    WHERE CBTEEG_CODIGO = ? and EGRE_NUMERO = ? and CBTEEGSUC_CODIGO = ? 
                                    AND EGRC_NUMRET != '0' AND RETEN_CODIGO = ? ";
                }
                DataTable registros = await _conn.ObtenerRegistrosAsync(consulta, parametrosAdd);
                if(registros is null || registros.Rows.Count == 0)
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
                string consulta = @$"SELECT PRO_LGLNOMBRE, PRO_LGLCUIT, PRO_LGLDIRECCION, PRO_LGLLOCALIDAD, PROVIN_CODIGO FROM PROVEEDORES WHERE PRO_CODIGO = ? ";
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
