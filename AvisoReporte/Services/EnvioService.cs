using AvisoReporte.Interfaces;
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
        private readonly string _HostApi = string.IsNullOrEmpty(ConfigurationManager.AppSettings["HostReportManager"]) ? "localhost" : ConfigurationManager.AppSettings["HostReportManager"];
        private readonly string _PortApi = string.IsNullOrEmpty(ConfigurationManager.AppSettings["PuertoReportManager"]) ? "7253" : ConfigurationManager.AppSettings["PuertoReportManager"];

        public async Task<string> LlamadoApiCinetReportManager(LlamadoDto llamadoDto)
        {
            try
            {
                HttpClient cliente = new HttpClient();

                string json = JsonConvert.SerializeObject(llamadoDto);
                StringContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage respuestaMsg = await cliente.PostAsync($"http://{_HostApi}:{_PortApi}/api/ReportManager/GenerarReporte", contenido);

                Log.Information($"Json enviado:\n {json}");

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
                HttpResponseMessage respuestaMsg = await cliente.PostAsync($"http://{_HostApi}:{_PortApi}/api/ReportManager/GenerarRetencion", contenido);

                Log.Information($"Json enviado:\n {json}");

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
