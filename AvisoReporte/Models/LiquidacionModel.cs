namespace AvisoReporte.Models
{
    public class LiquidacionModel
    {
        public string CodigoComprobante { get; set; }
        public string NumeroComprobanteFAC { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal Saldo { get; set; }
        public decimal ImportePagado { get; set; }
    }
}
