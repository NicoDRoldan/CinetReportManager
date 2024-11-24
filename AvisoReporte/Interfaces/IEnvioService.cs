using AvisoReporte.Models.DTO;
using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IEnvioService
    {
        Task<string> LlamadoApiCinetReportManager(LlamadoDto llamadoDto);
        Task<string> LlamadoApiCinetReportManager(List<RetencionModel> retenciones);
    }
}
