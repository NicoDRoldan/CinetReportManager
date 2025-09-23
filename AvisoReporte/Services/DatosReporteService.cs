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
using AvisoReporte.Models.Comprobante;

namespace AvisoReporte.Services
{
    public class DatosReporteService : IDatosReporteService
    {
        private readonly IAccesoDatosService _accesoDatosService;
        private readonly IMapeoDatosService _mapeoDatosService;

        private readonly DBConnect _conn;

        public DatosReporteService(DBConnect conn, IAccesoDatosService accesoDatosService, IMapeoDatosService mapeoDatosService)
        {
            _conn = conn;
            _accesoDatosService = accesoDatosService;
            _mapeoDatosService = mapeoDatosService;
        }

        public async Task<OrdenDePagoModel> ObtenerOrdenDePago(string cod_Comprobante, string num_Comprobante, string cod_Sucursal, string cod_Proveedor, bool enviaEmail = true, bool esConsulta = false)
        {
            List<string> parametros = new List<string> { cod_Comprobante, num_Comprobante, cod_Sucursal };
            try
            {
                DataTable datosComprobante = await _accesoDatosService.ObtenerDatosDeComprobante(parametros, esConsulta);
                if (datosComprobante is null || datosComprobante.Rows.Count == 0)
                    throw new Exception($"Comprobante {cod_Comprobante} - Número {num_Comprobante} - No se encontraron comprobantes.");
                DataTable datosProveedor = await _accesoDatosService.ObtenerDatosDeProveedor(cod_Proveedor);
                DataTable datosLiquidaciones = await _accesoDatosService.ObtenerDatosDeLiquidacion(parametros, cod_Proveedor);
                DataTable datosLiquidacionPlanCodigo = new();
                DataTable datosValoresIng = await _accesoDatosService.ObtenerDatosDeValores(parametros, true);
                DataTable datosValoresEgr = await _accesoDatosService.ObtenerDatosDeValores(parametros, false);

                if (datosLiquidaciones.Rows.Count == 0)
                    datosLiquidacionPlanCodigo = await _accesoDatosService.ObtenerDatosDeLiquidacionPlanCodigo(parametros);

                ClavesComprobantesModel claves = new ClavesComprobantesModel
                {
                    Cod_Comprobante = cod_Comprobante,
                    Num_Comprobante = num_Comprobante,
                    Cod_Sucursal = cod_Sucursal,
                    Cod_Proveedor = cod_Proveedor
                };

                OrdenDePagoModel ordenDePago = await _mapeoDatosService.MapeoOrdenDePago(datosComprobante, datosProveedor, datosLiquidaciones, datosLiquidacionPlanCodigo, datosValoresIng, datosValoresEgr, claves, enviaEmail, esConsulta);

                return ordenDePago;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener datos: {ex.Message}");
            }
        }
    }
}