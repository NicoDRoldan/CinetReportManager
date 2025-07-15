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

        public async Task<HttpResponseMessage> PostAsyncRequest(string json, string host, string port, string controller, string endpoint)
        {
            try
            {
                HttpClient client = new HttpClient();
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"http://{host}:{port}/api/{controller}/{endpoint}", content);
                Log.Information($"Json enviado:\n {json}");
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GenerarReporteResponse> LlamadoApiCinetReportManager(LlamadoDto llamadoDto)
        {
            try
            {
                string json = JsonConvert.SerializeObject(llamadoDto);

                HttpResponseMessage respuestaMsg = await PostAsyncRequest(json, _HostApi, _PortApi, "ReportManager", "GenerarReporte");

                string msgJson = await respuestaMsg.Content.ReadAsStringAsync();
                var respuestaApi = JsonConvert.DeserializeObject<GenerarReporteResponse>(msgJson);

                if (respuestaMsg.IsSuccessStatusCode)
                {
                    if (respuestaApi.Success)
                    { 
                        Log.Information(respuestaApi.Message);
                        Log.Information($"Archivos Generados:");

                        foreach(var archivo in respuestaApi.ArchivosGenerados)
                        {
                            Log.Information($"{archivo}");
                        }

                        return respuestaApi;
                    }
                    else
                    {
                        throw new Exception(respuestaApi.Message);
                    }
                }
                else
                {
                    throw new Exception(msgJson);
                }
            }
            catch(Exception ex) 
            {
                throw new Exception($"Error al hacer el llamado a CinetReportManager: {ex.Message}");
            }
        }

        public async Task<GenerarReporteResponse> LlamadoApiCinetReportManager(List<RetencionModel> retenciones)
        {
            try
            {
                string json = JsonConvert.SerializeObject(retenciones);

                HttpResponseMessage respuestaMsg = await PostAsyncRequest(json, _HostApi, _PortApi, "ReportManager", "GenerarRetencion");

                string msgJson = await respuestaMsg.Content.ReadAsStringAsync();
                var respuestaApi = JsonConvert.DeserializeObject<GenerarReporteResponse>(msgJson);

                if (respuestaMsg.IsSuccessStatusCode)
                {
                    Log.Information(respuestaApi.Message);
                    Log.Information($"Archivos Generados:");

                    foreach (var archivo in respuestaApi.ArchivosGenerados)
                    {
                        Log.Information($"{archivo}");
                    }

                    return respuestaApi;
                }
                else
                {
                    throw new Exception(msgJson);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al hacer el llamado a CinetReportManager: {ex.Message}");
            }
        }
    }
}
