using CinetReportManager.Helpers;
using CinetReportManager.Interfaces;
using CinetReportManager.Models;
using CinetReportManager.Models.Retenciones;
using Humanizer;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace CinetReportManager.Events
{
    public class FirmaHandlerReten : IEventHandler
    {
        private readonly IOrdenDePagoService _ordenDePagoService;

        RetencionModel _retencion;
        byte[] _firma;
        List<byte[]> _fuentes;
        string _aclaracion_firma;
        bool _reportePrincipal;

        public FirmaHandlerReten(RetencionModel retencion, List<byte[]> fuentes, byte[] firma, string aclaracion_firma, bool reportePrincipal)
        {
            _retencion = retencion;
            _firma = firma;
            _fuentes = fuentes;
            _aclaracion_firma = aclaracion_firma;
            _reportePrincipal = reportePrincipal;
        }

        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();

            ImageData imageData = ImageDataFactory.Create(_firma);
            Image firma = new Image(imageData);

            int numPagina = pdfDoc.GetPageNumber(docEvent.GetPage());

            Rectangle pageSizeFirma = new Rectangle(25, page.GetPageSize().GetTop() - 800, page.GetPageSize().GetWidth() - 72, 100);
            Rectangle pageSizeAclaracion = new Rectangle(36, page.GetPageSize().GetTop() - 890, page.GetPageSize().GetWidth() - 72, 100);
            Rectangle pageSizeImporte = new Rectangle(20, page.GetPageSize().GetTop() - 930, page.GetPageSize().GetWidth() - 72, 100);

            Canvas canvasFirma = new Canvas(page, pageSizeFirma);
            Canvas canvasAclaracion = new Canvas(page, pageSizeAclaracion);

            Cell celdaFirma = new Cell().Add(firma.SetWidth(80).SetHeight(120).SetHorizontalAlignment(HorizontalAlignment.RIGHT)).SetBorder(Border.NO_BORDER);
            canvasFirma.Add(celdaFirma);
            canvasFirma.Close();

            Cell celdaAclaracion = new Cell().Add(new Paragraph(_aclaracion_firma).SetWidth(120).SetFontSize(8).SetCharacterSpacing(1)
                .SetFont(PdfFontFactory.CreateFont(_fuentes[0], PdfEncodings.WINANSI)).SetTextAlignment(TextAlignment.CENTER).SetHorizontalAlignment(HorizontalAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER);
            canvasAclaracion.Add(celdaAclaracion);
            canvasAclaracion.Close();

            if (!_reportePrincipal)
            {
                Canvas canvasImporte = new Canvas(page, pageSizeImporte);

                Cell celdaImporte = new Cell().Add(new Paragraph($"Monto total de la Retención: $ {_retencion.ImporteRetencion}")
                    .SetMarginTop(-150).SetFontSize(10).SetFont(PdfFontFactory.CreateFont(_fuentes[3], PdfEncodings.WINANSI)));
                canvasImporte.Add(celdaImporte);
                canvasImporte.Close();
            }
        }
    }
}
