using AvisoReporte.Models.Retenciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Models
{
    public class LlamadoDto
    {
        public virtual OrdenDePagoModel OrdenDePago { get; set; }
        public virtual ICollection<RetencionModel>? Retenciones { get; set; }
    }
}
