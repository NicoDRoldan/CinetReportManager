using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IAccesoDatosService
    {
        /* Ordenes de Pago */
        Task<DataTable> ObtenerDatosDeComprobante(List<string> parametros, bool esConsulta = false);
        Task<DataTable> ObtenerDatosDeProveedor(string codProveedor);
        Task<DataTable> ObtenerDatosDeLiquidacion(List<string> parametros, string codProveedor);
        Task<DataTable> ObtenerDatosDeLiquidacionPlanCodigo(List<string> parametros);
        Task<DataTable> ObtenerDatosDeValores(List<string> parametros, bool inge_numero);

        /* Retenciones */
        Task<DataTable> ObtenerRetenciones(List<string> parametros, string? retencionFiltro = null);
        Task<DataTable> ObtenerDatosRetencion(string cod_concepto, string? cod_retencion);
        Task<string> ObtenerImportes(List<string> parametros, string num_retencion, string tipoDeImporte, string cod_concepto);
        Task<string> ObtenerImporteOrigina(List<string> parametros);
        Task<DataTable> ObtenerDatosProveedor(string cod_proveedor);
        Task<string> ObtenerCodigoConcepto(string pro_codigo, string cod_impuesto);
    }
}
