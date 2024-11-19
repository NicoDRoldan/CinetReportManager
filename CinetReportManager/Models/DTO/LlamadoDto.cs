using CinetReportManager.Models.Retenciones;

namespace CinetReportManager.Models.DTO
{
    public class LlamadoDto
    {
        public string? BaseEmpresa { get; set; }
        public virtual OrdenDePagoModel OrdenDePago { get; set; }
        public virtual ICollection<RetencionModel>? Retenciones { get; set; }
    }
}
