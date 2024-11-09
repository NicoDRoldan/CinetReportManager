namespace CinetReportManager.Models
{
    public class OrdenDePagoModel
    {
        public string CodigoComprobante { get; set; }
        public string NumeroComprobanteOPA { get; set; }
        public string CodigoSucursal { get; set; }
        public DateTime FechaOPA { get; set; }
        public string CodigoProveedor { get; set; }
        public string RazonSocialProveedor { get; set; }
        public List<string>? EmailsProveedores { get; set; }
        public virtual ICollection<LiquidacionModel> Liquidaciones { get; set; }
        public virtual ICollection<ValorModel>? ValoresIng { get; set; } = new List<ValorModel>();
        public virtual ICollection<ValorModel>? ValoresEgr { get; set; } = new List<ValorModel>();
    }
}
