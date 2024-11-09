using AvisoReporte.Interfaces;
using AvisoReporte.Models;
using AvisoReporte.Models.DTO;
using AvisoReporte.Models.Retenciones;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AvisoReporte.Services
{
    public class EnvioService : IEnvioService
    {
        private readonly string _PortApi = ConfigurationManager.AppSettings["PuertoReportManager"];

        public async Task<string> LlamadoApiCinetReportManager(LlamadoDto llamadoDto)
        {
            try
            {
                //string api = ConfigurationManager.AppSettings["PuertoReportManager"] ?? throw new Exception("No hay un puerto asignado para el llamado.");
                HttpClient cliente = new HttpClient();

                string json = JsonConvert.SerializeObject(llamadoDto);
                StringContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage respuestaMsg = await cliente.PostAsync($"http://localhost:{_PortApi}/api/ReportManager/GenerarReporte", contenido);
                
                string msgJson = await respuestaMsg.Content.ReadAsStringAsync();
                RespuestaApi respuestaApi = JsonConvert.DeserializeObject<RespuestaApi>(msgJson);

                if (respuestaMsg.IsSuccessStatusCode)
                {
                    return respuestaApi.Message;
                }
                else
                {
                    throw new Exception(respuestaApi.Message);
                }
            }
            catch(Exception ex) 
            {
                throw new Exception($"Error al hacer el llamado a CinetReportManager: {ex.Message}");
            }
        }

        public async Task<string> LlamadoApiCinetReportManager(List<RetencionModel> retenciones)
        {
            try
            {
                HttpClient cliente = new HttpClient();

                string json = JsonConvert.SerializeObject(retenciones);
                StringContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage respuestaMsg = await cliente.PostAsync($"http://localhost:{_PortApi}/api/ReportManager/GenerarRetencion", contenido);

                string msgJson = await respuestaMsg.Content.ReadAsStringAsync();
                RespuestaApi respuestaApi = JsonConvert.DeserializeObject<RespuestaApi>(msgJson);

                if (respuestaMsg.IsSuccessStatusCode)
                {
                    return respuestaApi.Message;
                }
                else
                {
                    throw new Exception(respuestaApi.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al hacer el llamado a CinetReportManager: {ex.Message}");
            }
        }
    }
}
