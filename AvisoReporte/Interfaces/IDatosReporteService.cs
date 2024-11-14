using AvisoReporte.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IDatosReporteService
    {
        Task<OrdenDePagoModel> ObtenerOrdenDePago(string cod_Comprobante, string num_Comprobante, string cod_Sucursal, string cod_Proveedor, bool enviaEmail = true, bool esConsulta = false);
        Task<ClavesComprobantesModel> ObtenerClavesDeComprobante(string claves);
    }
}
