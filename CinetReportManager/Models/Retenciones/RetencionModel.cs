namespace CinetReportManager.Models.Retenciones
{
    public class RetencionModel
    {
        public string NumeroRetencion { get; set; }
        public string CodigoRetencion { get; set; }
        public DateTime Fecha { get; set; }
        public string IB { get; set; }
        public virtual AgenteRetencionModel AgenteRetencion { get; set; }
        public virtual SujetoRetenidoModel SujetoRetenido { get; set; }
        public virtual RetencionPracticadaModel RetencionPracticada { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal ImporteRetencion { get; set; }
        public string Firma { get; set; }
    }
}
