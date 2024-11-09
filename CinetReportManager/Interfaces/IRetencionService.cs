using CinetReportManager.Models.Retenciones;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace CinetReportManager.Interfaces
{
    public interface IRetencionService
    {
        Table DatosGenerales(RetencionModel retencion, List<byte[]> fuentes, bool reportePrincipal = true);
        void DatosAgenteDeRetencion(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true);
        void DatosSujetoRetenido(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true);
        void DatosRetencionPracticada(RetencionModel retencion, List<byte[]> fuentes, Document doc, string Nombre_Impuesto, string Nombre_Comprobante, bool reportePrincipal = true);
        void PorcentajesAPagar(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true);
    }
}
