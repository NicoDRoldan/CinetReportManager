using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Reflection;

namespace CinetReportManager.Helpers
{
    public static class Funciones
    {
        public static async Task<byte[]> ObtenerRecurso(string rutaRecurso)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(rutaRecurso) ?? throw new Exception("Error la cargar el recurso");
            byte[] recurso;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                recurso = ms.ToArray();
            }
            return recurso;
        }

        public static Cell CrearCelda(string dato, int fontSize, int characterSpacing = 0, int minWidth = 0, int maxWidth = 0, bool border = false, int borderTop = 0, TextAlignment? textAlignment = null, string? font = null)
        {
            Paragraph paragraph = new Paragraph(dato).SetFontSize(fontSize);
            Cell cell = new Cell().Add(paragraph);
            if (characterSpacing > 0) paragraph.SetCharacterSpacing(characterSpacing);
            if (!border) cell.SetBorder(Border.NO_BORDER);
            if (borderTop > 0) cell.SetBorderTop(new SolidBorder(borderTop));
            if (minWidth > 0) cell.SetMinWidth(minWidth);
            if (maxWidth > 0) cell.SetMaxWidth(maxWidth);
            if (textAlignment is not null) cell.SetTextAlignment(textAlignment);
            if (font == "HELVETICA_BOLD") cell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD));
            return cell;
        }
    }
}
