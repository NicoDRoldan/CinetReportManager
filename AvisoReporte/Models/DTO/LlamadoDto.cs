using AvisoReporte.Models.Comprobante;
using AvisoReporte.Models.Retenciones;

namespace AvisoReporte.Models.DTO
{
    public class LlamadoDto
    {
        public string? BaseEmpresa { get; set; }
        public virtual OrdenDePagoModel OrdenDePago { get; set; }
        public virtual ICollection<RetencionModel>? Retenciones { get; set; }
    }
}
