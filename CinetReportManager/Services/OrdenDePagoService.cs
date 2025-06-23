using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using CinetReportManager.Models;
using CinetReportManager.Helpers;
using CinetReportManager.Interfaces;
using System.Globalization;

namespace CinetReportManager.Services
{
    public class OrdenDePagoService : IOrdenDePagoService
    {
        int Width_Column_Fecha = 18;
        int Width_Column_Comprobante_Desc = 150;
        int Width_Column_Saldo = 35;
        int Width_Column_Pago_Importe = 35;
        int General_Size = 9;
        int Header_Size = 10;

        string _Razon_Social_Empresa;
        string _Direccion_Empresa;
        string _Telefono_Empresa;
        string _Tipo_Comprobante;

        public Table TablaEncabezado(OrdenDePagoModel ordenDePago, byte[] rutaImagen, byte[] rutaFont)
        {
            _Razon_Social_Empresa = "MOSTAZA Y PAN S.A.";
            _Direccion_Empresa = "Av Ing Huergo 953 P.11 CABA C1107AOJ";
            _Telefono_Empresa = "3754-2200";
            _Tipo_Comprobante = "Orden de Pago";

            // Se crea la tabla con 3 columnas.
            Table tablaEncabezado = new Table(new float[] { 1, 3, 1 });
            tablaEncabezado.SetWidth(UnitValue.CreatePercentValue(100)); // Se setea ancho de la tabla

            ImageData imgData = ImageDataFactory.Create(rutaImagen);
            // Se crea imagen y se le setea un ancho y alto.
            Image imgLogo = new Image(imgData).SetWidth(100).SetHeight(80);

            // Se crea una celda a la cual se le agrega el logo de la imagen.
            Cell celdaLogo = new Cell().Add(imgLogo)
                .SetBorder(Border.NO_BORDER); // Se quita el borde de la celda.
            tablaEncabezado.AddCell(celdaLogo); // Se agrega la celda a la tabla.

            /* La tabla 'tablaDatosEmpresa' es una tabla que estará contenida dentro
             * de una celda que a su vez estará contenida dentro de otra tabla */
            Table tablaDatosEmpresa = new Table(1);
            Cell celdaDatosEmpresa = new Cell().SetBorder(Border.NO_BORDER);

            tablaDatosEmpresa.AddCell(new Cell().Add(new Paragraph(_Razon_Social_Empresa)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18)
                .SetFont(PdfFontFactory.CreateFont(rutaFont, PdfEncodings.WINANSI)));
            tablaDatosEmpresa.AddCell(new Cell().Add(new Paragraph(_Direccion_Empresa)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetFontSize(10));
            tablaDatosEmpresa.AddCell(new Cell().Add(new Paragraph($"Teléfono: {_Telefono_Empresa}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetFontSize(10));

            celdaDatosEmpresa.Add(tablaDatosEmpresa);
            tablaEncabezado.AddCell(celdaDatosEmpresa);

            Table tablaInfoComprobante = new Table(1);
            Cell celdaInfoComprobante = new Cell().SetBorder(Border.NO_BORDER);

            tablaInfoComprobante.AddCell(new Cell().Add(new Paragraph($"{_Tipo_Comprobante} {ordenDePago.NumeroComprobanteOPA}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(12));
            tablaInfoComprobante.AddCell(new Cell().Add(new Paragraph($"Fecha: {DateOnly.FromDateTime(ordenDePago.FechaOPA)}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10)
                .SetPaddingTop(20).SetPaddingRight(10)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

            celdaInfoComprobante.Add(tablaInfoComprobante);
            tablaEncabezado.AddCell(celdaInfoComprobante);

            tablaEncabezado.SetBorderBottom(new SolidBorder(0.5f));

            return tablaEncabezado;
        }

        public Table TablaEncabezado(OrdenDePagoModel ordenDePago, List<byte[]> rutaFont, string baseEmpresa)
        {
            if(baseEmpresa.ToUpper() == "GALDEANO_ERP")
            {
                _Razon_Social_Empresa = "GALDEANO ALVARADO CHRISTIAN";
                _Direccion_Empresa = "COSSETTINI,OLGA 152 Piso:8 Dpto:7\r\nCIUDAD AUTONOMA BUENOS AIRES\r\n";
                _Tipo_Comprobante = "Orden de Pago";
            }
            else if(baseEmpresa.ToUpper() == "GADA_GROUP_ERP")
            {
                _Razon_Social_Empresa = "GADA GROUP";
                _Direccion_Empresa = "COSSETTINI,OLGA 152 Piso:8 Dpto:7\r\nCIUDAD AUTONOMA BUENOS AIRES\r\n";
                _Tipo_Comprobante = "Orden de Pago";
            }

            Table tablaEncabezado = new Table(new float[] { 3, 1 });
            tablaEncabezado.SetWidth(UnitValue.CreatePercentValue(100));

            Table tablaDatosEmpresa = new Table(1);
            Cell celdaDatosEmpresa = new Cell().SetBorder(Border.NO_BORDER);

            tablaDatosEmpresa.AddCell(new Cell().Add(new Paragraph(_Razon_Social_Empresa)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT).SetFontSize(18)
                .SetFont(PdfFontFactory.CreateFont(rutaFont[0], PdfEncodings.WINANSI)));
            tablaDatosEmpresa.AddCell(new Cell().Add(new Paragraph(_Direccion_Empresa)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT).SetFontSize(10)
                .SetPaddingTop(-5)
                .SetFont(PdfFontFactory.CreateFont(rutaFont[1], PdfEncodings.WINANSI)));

            celdaDatosEmpresa.Add(tablaDatosEmpresa);
            tablaEncabezado.AddCell(celdaDatosEmpresa);

            Table tablaInfoComprobante = new Table(1);
            Cell celdaInfoComprobante = new Cell().SetBorder(Border.NO_BORDER);

            tablaInfoComprobante.AddCell(new Cell().Add(new Paragraph($"{_Tipo_Comprobante} {ordenDePago.NumeroComprobanteOPA}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(12));
            tablaInfoComprobante.AddCell(new Cell().Add(new Paragraph($"Fecha: {DateOnly.FromDateTime(ordenDePago.FechaOPA)}")).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10)
                .SetPaddingTop(20).SetPaddingRight(10)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

            celdaInfoComprobante.Add(tablaInfoComprobante);
            celdaInfoComprobante.Add(new Paragraph("").SetMarginBottom(21));
            tablaEncabezado.AddCell(celdaInfoComprobante);

            tablaEncabezado.SetBorderBottom(new SolidBorder(0.5f));

            return tablaEncabezado;
        }

        public Table TablaProveedor(OrdenDePagoModel ordenDePago)
        {
            Table tablaProveedor = new Table(new float[] { 2, 1 }).SetWidth(500);
            List<string> datos = new List<string> { $"Señores: {ordenDePago.RazonSocialProveedor}", $"CTA: {ordenDePago.CodigoProveedor}" };

            foreach (string dato in datos)
            {
                tablaProveedor.AddCell(Funciones.CrearCelda(dato, 10, characterSpacing: 1));
            }
            return tablaProveedor;
        }

        public Table TablaLiquidacionesHeader()
        {
            var tablaLiquidacionHeader = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorderTop(null);

            Cell fechaCell = new Cell().Add(new Paragraph("Fecha"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha);
            tablaLiquidacionHeader.AddHeaderCell(fechaCell);

            Cell comprobanteCell = new Cell().Add(new Paragraph("Comprobante"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc);
            tablaLiquidacionHeader.AddHeaderCell(comprobanteCell);

            Cell saldoCell = new Cell().Add(new Paragraph("Saldo"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo);
            tablaLiquidacionHeader.AddHeaderCell(saldoCell);

            Cell pagoCell = new Cell().Add(new Paragraph("Pago"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe);
            tablaLiquidacionHeader.AddHeaderCell(pagoCell);

            return tablaLiquidacionHeader;
        }

        public Table TablaLiquidaciones(OrdenDePagoModel ordenDePago)
        {
            //Tabla Liquidación en Pesos
            var tablaLiquidacion = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorderTop(null);

            // Agregar celdas con los datos de las facturas:
            decimal Total_Pago_Facturas = ordenDePago.Liquidaciones.Sum(s => s.ImportePagado);
            string Total_Pago_Facturas_Format = Total_Pago_Facturas.ToString("#,##0.00", CultureInfo.InvariantCulture);

            decimal Sum_Facturas = ordenDePago.Liquidaciones.Count();
            foreach (var fac in ordenDePago.Liquidaciones)
            {
                string saldoFac = fac.Saldo == 0 ? "" : Convert.ToString(fac.Saldo);
                string Importe_Pagado = fac.ImportePagado.ToString("#,##0.00", CultureInfo.InvariantCulture);

                tablaLiquidacion.AddCell(new Cell().Add(new Paragraph(Convert.ToString(DateOnly.FromDateTime(fac.FechaFactura)))).SetFontSize(General_Size).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"{fac.CodigoComprobante} {fac.NumeroComprobanteFAC}").SetMarginLeft(5)).SetFontSize(General_Size).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"{saldoFac}")).SetFontSize(General_Size).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"{Importe_Pagado}")).SetFontSize(General_Size).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
            }
            tablaLiquidacion.AddCell(new Cell().Add(new Paragraph("Son:")).SetFontSize(General_Size).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha).SetBorder(Border.NO_BORDER));
            tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"{Sum_Facturas}").SetMarginLeft(5)).SetFontSize(General_Size).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc).SetBorder(Border.NO_BORDER));
            tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"TOTAL")).SetFontSize(General_Size).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));
            tablaLiquidacion.AddCell(new Cell().Add(new Paragraph($"{Total_Pago_Facturas_Format}")).SetFontSize(General_Size).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetBorderTop(new SolidBorder(1))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

            return tablaLiquidacion;
        }

        public Table TablaValoresHeader()
        {
            var tablaValores = new Table(4)
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorderTop(null);

            Cell fechaValCel = new Cell().Add(new Paragraph("Fecha"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha);
            tablaValores.AddHeaderCell(fechaValCel);

            Cell descripcionValCel = new Cell().Add(new Paragraph("Descripción"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc);
            tablaValores.AddHeaderCell(descripcionValCel);

            Cell saldoValCel = new Cell().Add(new Paragraph(""))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo);
            tablaValores.AddHeaderCell(saldoValCel);

            Cell importeValCel = new Cell().Add(new Paragraph("Importe"))
                .SetFontSize(Header_Size).SetPaddingLeft(5).SetBorderTop(null).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe);
            tablaValores.AddHeaderCell(importeValCel);

            return tablaValores;
        }

        public Table TablaValores(OrdenDePagoModel ordenDePago, bool inge_numero)
        {
            var valores = new List<ValorModel>();

            var valoresAgrupados = ordenDePago.ValoresIng.GroupBy(g => g.Descripcion)
                .Select(s => new
                {
                    Descripcion = s.Key,
                    Count = s.Count()
                }).ToList();

            if (inge_numero)
            {
                valores = ordenDePago.ValoresIng.ToList();
            }
            else
            {
                valores = ordenDePago.ValoresEgr.ToList();
                valoresAgrupados = ordenDePago.ValoresEgr.GroupBy(g => g.Descripcion)
                .Select(s => new
                {
                    Descripcion = s.Key,
                    Count = s.Count()
                }).ToList();
            }

            var tablaValores = new Table(4)
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorderTop(null);

            decimal Total_Importe_Valores = 0;

            foreach (var valG in valoresAgrupados)
            {
                foreach (var val in valores)
                {
                    if (val.Descripcion == valG.Descripcion)
                    {
                        string Val_Importe_Format = val.Importe.ToString("#,##0.00", CultureInfo.InvariantCulture);
                        tablaValores.AddCell(new Cell().Add(new Paragraph(Convert.ToString(DateOnly.FromDateTime(val.FechaValor)))).SetFontSize(General_Size).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                        tablaValores.AddCell(new Cell().Add(new Paragraph($"{val.Descripcion}   {val.NumMovimiento}").SetMarginLeft(5)).SetFontSize(General_Size).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                        tablaValores.AddCell(new Cell().Add(new Paragraph($"")).SetFontSize(General_Size).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                        tablaValores.AddCell(new Cell().Add(new Paragraph($"{Val_Importe_Format}")).SetFontSize(General_Size).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPaddingBottom(0).SetPaddingTop(0));
                        Total_Importe_Valores += val.Importe;
                        continue;
                    }
                }
                string Total_Importe_Valores_Format = Total_Importe_Valores.ToString("#,##0.00", CultureInfo.InvariantCulture);
                tablaValores.AddCell(new Cell().Add(new Paragraph("Son:")).SetFontSize(General_Size).SetMinWidth(Width_Column_Fecha).SetMaxWidth(Width_Column_Fecha).SetBorder(Border.NO_BORDER));
                tablaValores.AddCell(new Cell().Add(new Paragraph($"{1}").SetMarginLeft(5)).SetFontSize(General_Size).SetMinWidth(Width_Column_Comprobante_Desc).SetMaxWidth(Width_Column_Comprobante_Desc).SetBorder(Border.NO_BORDER));
                tablaValores.AddCell(new Cell().Add(new Paragraph($"TOTAL")).SetFontSize(General_Size).SetMinWidth(Width_Column_Saldo).SetMaxWidth(Width_Column_Saldo).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));
                tablaValores.AddCell(new Cell().Add(new Paragraph($"{Total_Importe_Valores_Format}")).SetFontSize(General_Size).SetMinWidth(Width_Column_Pago_Importe).SetMaxWidth(Width_Column_Pago_Importe).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetBorderTop(new SolidBorder(1)).SetPaddingBottom(5)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));
                Total_Importe_Valores = 0;
            }
            return tablaValores;
        }

    }
}
