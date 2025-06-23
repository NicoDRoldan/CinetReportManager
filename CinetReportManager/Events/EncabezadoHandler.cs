using CinetReportManager.Helpers;
using CinetReportManager.Interfaces;
using CinetReportManager.Models;
using Humanizer;
using iText.IO.Font;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace CinetReportManager.Events
{
    public class EncabezadoHandler : IEventHandler
    {
        private readonly IOrdenDePagoService _ordenDePagoService;

        OrdenDePagoModel _ordenDepago;
        Document _document;
        byte[] _rutaImagen;
        List<byte[]> _fonts;
        string? _baseEmpresa;

        public EncabezadoHandler(IOrdenDePagoService ordenDePagoService, OrdenDePagoModel ordenDepago, Document document, byte[] rutaImagen, List<byte[]> fonts
            , string? baseEmpresa = null)
        {
            _ordenDePagoService = ordenDePagoService;
            _ordenDepago = ordenDepago;
            _document = document;
            _rutaImagen = rutaImagen;
            _fonts = fonts;
            _baseEmpresa = baseEmpresa;
        }

        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();

            Rectangle rootArea = new Rectangle(
                5,  // margen izquierdo
                page.GetPageSize().GetTop() - 105, // margen superior
                page.GetPageSize().GetWidth() - 15, // ancho
                100  // altura del encabezado
            );
            Canvas canvas = new Canvas(page, rootArea);

            // Si la empresa es GALDEANO_ERP, la cabecera será diferente.
            if (!string.IsNullOrEmpty(_baseEmpresa) && (_baseEmpresa == "GALDEANO_ERP" || _baseEmpresa == "GADA_GROUP_ERP"))
                canvas.Add(_ordenDePagoService.TablaEncabezado(_ordenDepago, _fonts, _baseEmpresa));
            else
                canvas.Add(_ordenDePagoService.TablaEncabezado(_ordenDepago, _rutaImagen, _fonts[0]));

            Canvas canvas1 = new Canvas(page, rootArea.SetY(650));
            canvas1.Add(_ordenDePagoService.TablaProveedor(_ordenDepago));

            // Generación de parrafos para firma, aclaración y documento.
            // Texto de recibo y observaciones
            decimal Total_Pago_Facturas = 0.00m;
            if (_ordenDepago.Liquidaciones.Any())
            {
                Total_Pago_Facturas = _ordenDepago.Liquidaciones.Sum(s => s.ImportePagado);
            }
            string sumTotalString = Total_Pago_Facturas.ToString();
            int decimalTotal = Convert.ToInt32(sumTotalString.Substring(sumTotalString.LastIndexOf(',')).Replace(',', ' '));
            string sumaTotalEnLetras = Convert.ToInt32(Total_Pago_Facturas).ToWords(new System.Globalization.CultureInfo("es"));
            string decimalesEnLetras = decimalTotal.ToWords(new System.Globalization.CultureInfo("es"));

            Paragraph sumaTotalEnLetrasParagraph = new Paragraph($"Recibí la suma de Pesos\n {sumaTotalEnLetras} con {decimalesEnLetras} centavos.").SetTextAlignment(TextAlignment.LEFT)
                .SetCharacterSpacing(1).SetFontSize(9).SetFont(PdfFontFactory.CreateFont(_fonts[1], PdfEncodings.WINANSI))
                .SetWidth(page.GetPageSize().GetWidth() - 15);
            Paragraph observaciones = new Paragraph(@"Observaciones").SetTextAlignment(TextAlignment.LEFT).SetFont(PdfFontFactory.CreateFont(_fonts[1], PdfEncodings.WINANSI));
            Paragraph firmaParagraph = new Paragraph(@"Firma            __________________________").SetTextAlignment(TextAlignment.RIGHT);
            Paragraph aclaracionParagraph = new Paragraph(@"Aclaración    __________________________").SetTextAlignment(TextAlignment.RIGHT);
            Paragraph documentoParagraph = new Paragraph(@"Documento   __________________________").SetTextAlignment(TextAlignment.RIGHT);

            // Obtener tamaños x y.
            // Obtener tamaño de página
            Rectangle pageSize = pdfDoc.GetDefaultPageSize();
            float x = pageSize.GetWidth();
            float y = pageSize.GetBottom() + 20;
            
            canvas.ShowTextAligned(sumaTotalEnLetrasParagraph, 5, 200, TextAlignment.LEFT);
            canvas.ShowTextAligned(observaciones, 5, 180, TextAlignment.LEFT);
            canvas.ShowTextAligned(firmaParagraph, x - 30, y + 80, TextAlignment.RIGHT);
            canvas.ShowTextAligned(aclaracionParagraph, x - 30, y + 40, TextAlignment.RIGHT);
            canvas.ShowTextAligned(documentoParagraph, x - 30, y + 0, TextAlignment.RIGHT);
            canvas.Close();
        }
    }
}
