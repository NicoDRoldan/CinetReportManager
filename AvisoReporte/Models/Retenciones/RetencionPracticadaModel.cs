namespace AvisoReporte.Models.Retenciones
{
    public class RetencionPracticadaModel
    {
        public string TipoImpuesto { get; set; } // Ej: Ingresos Brutos de la Prov. de Bs. As
        public string TipoComprobante { get; set; } // Código del comprobante.
        public string NumeroComprobante { get; set; } // Número del, en este caso, Egreso.
        public string DescripcionReten { get; set; } // RETEN_DESCCONCEPTO de RETEN_TABLA (Regimen).
        public decimal ImporteOriginaReten { get; set; } // Importe del último renglon de Egresos_D.
    }
}
