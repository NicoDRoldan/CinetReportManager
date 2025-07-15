using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Interfaces
{
    public interface IPrintService
    {
        Task ImprimirReporte(List<string> rutaReportesPdf);
    }
}
