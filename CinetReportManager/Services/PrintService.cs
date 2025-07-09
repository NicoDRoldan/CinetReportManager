using CinetReportManager.Interfaces;
using PdfiumPrinter;
using System.Drawing.Printing;

namespace CinetReportManager.Services
{
    public class PrintService : IPrintService
    {
        public async Task ImprimirReporte(string pathArchivo)
        {
            try
            {
                string impresora = new PrinterSettings().PrinterName;
                var pdfPrinter = new PdfPrinter(impresora);
                pdfPrinter.Print(pathArchivo);
            }
            catch(Exception ex)
            {
                throw new Exception("Error al imprimir");
            }
        }
    }
}
