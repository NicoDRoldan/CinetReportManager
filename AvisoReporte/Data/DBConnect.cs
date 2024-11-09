using System.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;

namespace AvisoReporte.Data
{
    public class DBConnect
    {
        private static string Base_Odbc { get; set; }
        private static string User_Odbc { get; set; }
        private static string Pass_Odbc { get; set; }
        private static string Tipo_Conexion_Config { get; set; }
        private static string Empresa_Config { get; set; }

        public async Task<string> ObtenerStringConexion()
        {
            ConfigurarDatosEmpresa();
            User_Odbc = "SA";
            return $"DSN={Base_Odbc};UID={User_Odbc};PWD={Pass_Odbc}";
        }

        public async Task ConfigurarDatosEmpresa()
        {
            Tipo_Conexion_Config = ConfigurationManager.AppSettings["TipoConexion"] is null ? "2" : ConfigurationManager.AppSettings["TipoConexion"];
            Empresa_Config = ConfigurationManager.AppSettings["Empresa"] is null ? "2" : ConfigurationManager.AppSettings["Empresa"];

            switch (Tipo_Conexion_Config)
            {
                case "1":
                    Pass_Odbc = "cinettorcel";
                    break;
                case "2":
                    Pass_Odbc = "Cinet1212";
                    break;
                default:
                    throw new Exception("No se estableció la conexión.");
            }

            switch (Empresa_Config)
            {
                case "1":
                    Base_Odbc = "Backoffice";
                    break;
                case "2":
                    Base_Odbc = "Cinet_PDV";
                    break;
                case "3":
                    Base_Odbc = "MOSTAZA_ERP";
                    break;
                case "4":
                    Base_Odbc = "FRQ_ERP";
                    break;
                case "5":
                    Base_Odbc = "PROSPEROUS_ERP";
                    break;
                case "6":
                    Base_Odbc = "DAFIRUZ_ERP";
                    break;
                case "100":
                    Base_Odbc = "TEST_ERP";
                    break;
                default: 
                    throw new Exception("No se indicó una Empresa a la que conectarse.");
            }
        }

        public async Task<OdbcConnection> AbrirConexionAsync()
        {
            try
            {
                var stringConnect = await ObtenerStringConexion();
                var conn = new OdbcConnection(stringConnect);
                await conn.OpenAsync();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        public async Task EjecutarConsultaAsync(string query)
        {
            try
            {
                var conn = await AbrirConexionAsync();
                var cmd = new OdbcCommand(query, conn);
                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar la consulta: {ex.Message}");
            }
        }

        public async Task<DataTable> ObtenerRegistrosAsync(string query, List<string> parameters)
        {
            var tabla = new DataTable();
            try
            {
                var conn = await AbrirConexionAsync();
                var cmd = new OdbcCommand(query, conn);
                foreach ( var item in parameters)
                {
                    cmd.Parameters.AddWithValue("?", item);
                }
                var da = new OdbcDataAdapter(cmd);
                da.Fill(tabla);
                da.Dispose();
                await conn.CloseAsync();
                return tabla;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar la consulta: {ex.Message}");
            }
        }

        public async Task<string> ObtenerRegistroAsync(string query, List<string> parameters)
        {
            try
            {
                var conn = await AbrirConexionAsync();
                var cmd = new OdbcCommand(query, conn);
                foreach (var item in parameters)
                {
                    cmd.Parameters.AddWithValue("?", item);
                }
                var resultado = await cmd.ExecuteScalarAsync();
                await conn.CloseAsync();
                if (resultado is null || string.IsNullOrEmpty(resultado.ToString())) throw new Exception("No se obtuvo resultados de la consulta");
                return resultado.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al ejecutar la consulta: {ex.Message}");
            }
        }
    }
}
