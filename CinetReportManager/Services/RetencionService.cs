using CinetReportManager.Interfaces;
using CinetReportManager.Models.Retenciones;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CinetReportManager.Services
{
    public class RetencionService : IRetencionService
    {
        public Table DatosGenerales(RetencionModel retencion, List<byte[]> fuentes, bool reportePrincipal = true)
        {
            Table tablaDatosGenerales = new Table(new float[] { 1, 1 });

            Paragraph parfCertificado = new Paragraph($"Certificado Número: ");
            Paragraph parfFecha = new Paragraph($"Fecha: ");
            Paragraph parfIB = new Paragraph($"IB: ");

            Paragraph parfValorCertificado = new Paragraph($"{retencion.NumeroRetencion}").SetTextAlignment(TextAlignment.RIGHT);
            Paragraph parfValorFecha = new Paragraph($"{DateOnly.FromDateTime(retencion.Fecha)}").SetTextAlignment(TextAlignment.RIGHT);
            Paragraph parfValorIB = new Paragraph($"{retencion.IB}").SetTextAlignment(TextAlignment.RIGHT); ;

            Cell celdaGenerales = new Cell().SetBorder(Border.NO_BORDER);
            Cell celdaGeneralesValores = new Cell().SetBorder(Border.NO_BORDER);

            if (reportePrincipal)
            {
                celdaGenerales.Add(parfCertificado);
                celdaGenerales.Add(parfFecha);
                celdaGenerales.Add(parfIB);

                celdaGeneralesValores.Add(parfValorCertificado
                    .SetFont(PdfFontFactory.CreateFont(fuentes[1], PdfEncodings.WINANSI)));
                celdaGeneralesValores.Add(parfValorFecha);
                celdaGeneralesValores.Add(parfValorIB);
            }
            else
            {
                parfFecha.SetTextAlignment(TextAlignment.RIGHT);
                parfValorFecha.SetTextAlignment(TextAlignment.RIGHT);

                celdaGenerales.Add(parfCertificado.SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)));
                celdaGenerales.Add(new Paragraph($"").SetMarginTop(10));
                celdaGenerales.Add(parfFecha.SetMarginRight(7).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)));

                celdaGeneralesValores.Add(parfValorCertificado);
                celdaGeneralesValores.Add(new Paragraph($"").SetMarginTop(12));
                celdaGeneralesValores.Add(parfValorFecha);
            }

            tablaDatosGenerales.AddCell(celdaGenerales);
            tablaDatosGenerales.AddCell(celdaGeneralesValores);

            tablaDatosGenerales
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                .SetMarginRight(40)
                .SetMarginBottom(-10);

            return tablaDatosGenerales;
        }

        public void DatosAgenteDeRetencion(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true)
        {
            if (reportePrincipal)
            {
                doc.Add(new Paragraph("Datos del Agente de Retención").SetPadding(0).SetFont(PdfFontFactory.CreateFont(fuentes[1], PdfEncodings.WINANSI)).SetUnderline());
                doc.Add(new Paragraph($"Denominación: {retencion.AgenteRetencion.Denominacion}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"{retencion.AgenteRetencion.DireccionAgente}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"IVA: {retencion.AgenteRetencion.IvaAgente}   CUIT N°: {retencion.AgenteRetencion.CuitAgente}").SetPadding(0).SetMargin(0));
            }
            else
            {
                doc.Add(new Paragraph("A) Datos del Agente de Retención").SetPadding(0).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)).SetFontSize(15));

                doc.Add(new Paragraph($"Denominación: {retencion.AgenteRetencion.Denominacion.ToUpper()}").SetMargin(0).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("CUIT N°: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.AgenteRetencion.CuitAgente}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Dirección: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.AgenteRetencion.DireccionAgente}")));
            }

            Paragraph espacioVacio = new Paragraph().SetMarginBottom(10);
            doc.Add(espacioVacio);
        }

        public void DatosSujetoRetenido(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true)
        {
            if (reportePrincipal)
            {
                doc.Add(new Paragraph("Datos del Sujeto Retenido").SetPadding(0)
                            .SetFont(PdfFontFactory.CreateFont(fuentes[1], PdfEncodings.WINANSI))
                            .SetUnderline());
                doc.Add(new Paragraph($"Apellido y Nombre o Denominación: {retencion.SujetoRetenido.RazonSocialSujeto}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"CUIT N°: {retencion.SujetoRetenido.CuitSujeto}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"Dirección: {retencion.SujetoRetenido.DireccionRetenido}").SetPadding(0).SetMargin(0));
            }
            else
            {
                doc.Add(new Paragraph("B) Datos del Sujeto Retenido").SetPadding(0).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)).SetFontSize(15));

                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Apellido y Nombre o Denominación: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.SujetoRetenido.RazonSocialSujeto}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("CUIT N°: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.SujetoRetenido.CuitSujeto}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Dirección: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.SujetoRetenido.DireccionRetenido}")));
            }

            Paragraph espacioVacio = new Paragraph().SetMarginBottom(10);
            doc.Add(espacioVacio);
        }

        public void DatosRetencionPracticada(RetencionModel retencion, List<byte[]> fuentes, Document doc, string Nombre_Impuesto, string Nombre_Comprobante, bool reportePrincipal = true)
        {
            if (reportePrincipal)
            {
                doc.Add(new Paragraph("Datos de la Retención Practicada").SetPadding(0)
                            .SetFont(PdfFontFactory.CreateFont(fuentes[1], PdfEncodings.WINANSI))
                            .SetUnderline());
                doc.Add(new Paragraph($"Impuesto: {Nombre_Impuesto}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"Comprobante que origina la retención: {Nombre_Comprobante} N° {retencion.RetencionPracticada.NumeroComprobante}").SetPadding(0).SetMargin(0));
                doc.Add(new Paragraph($"Monto del comprobante que origina la retención: {retencion.RetencionPracticada.ImporteOriginaReten}").SetPadding(0).SetMargin(0));
            }
            else
            {
                doc.Add(new Paragraph("C) Datos de la Retención Practicada").SetPadding(0).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)).SetFontSize(15));

                doc.Add(new Paragraph($"Impuesto: {Nombre_Impuesto}").SetMargin(0).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Régimen: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.RetencionPracticada.DescripcionReten}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Comprobante que origina la retención: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{Nombre_Comprobante} N° {retencion.RetencionPracticada.NumeroComprobante}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Monto del comprobante que origina la retención: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.RetencionPracticada.ImporteOriginaReten}")));
                doc.Add(new Paragraph().SetMargin(0)
                    .Add(new Text("Monto de la retención: ").SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)))
                    .Add(new Text($"{retencion.ImporteRetencion}")));
            }

            Paragraph espacioVacio = new Paragraph().SetMarginBottom(10);
            doc.Add(espacioVacio);
        }

        public void PorcentajesAPagar(RetencionModel retencion, List<byte[]> fuentes, Document doc, bool reportePrincipal = true)
        {
            doc.Add(new Paragraph($"Base Imponible: {retencion.BaseImponible}").SetFontSize(10).SetFont(PdfFontFactory.CreateFont(fuentes[2], PdfEncodings.WINANSI)));
            doc.Add(new Paragraph($"Porcentaje: {retencion.Porcentaje}%").SetFontSize(10).SetFont(PdfFontFactory.CreateFont(fuentes[2], PdfEncodings.WINANSI)));
            doc.Add(new Paragraph($"Importe Retencion: {retencion.ImporteRetencion}").SetFontSize(10).SetFont(PdfFontFactory.CreateFont(fuentes[3], PdfEncodings.WINANSI)));
        }
    }
}
