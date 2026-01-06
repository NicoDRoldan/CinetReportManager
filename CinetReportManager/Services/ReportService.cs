using iText.IO.Image;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using CinetReportManager.Models;
using CinetReportManager.Interfaces;
using Microsoft.AspNetCore.Server.IIS.Core;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;
using Humanizer;
using System.IO.Compression;
using System.Reflection;
using CinetReportManager.Helpers;
using System.Drawing.Printing;
using System.Diagnostics;
using iText.Kernel.Pdf.Canvas.Wmf;
using CinetReportManager.Models.Retenciones;
using iText.Signatures.Validation.V1;
using iText.Kernel.Events;
using iText.Kernel.Pdf.Canvas;
using CinetReportManager.Events;
using CinetReportManager.Models.DTO;
using System.Text;

namespace CinetReportManager.Services
{
    public class ReportService : IReportService
    {
        private readonly IOrdenDePagoService _ordenDePagoService;
        private readonly IRetencionService _retencionService;
        private readonly IConfiguration _configuration;
        private readonly IPrintService _printService;
        private readonly IEmailService _emailService;

        private string _numeroComprobante;
        private string _codComprobante;
        private string _codProveedor;
        private string _rutaReporte;

        public ReportService(IOrdenDePagoService ordenDePagoService, IRetencionService retencionService, IConfiguration configuration, IPrintService printService, IEmailService emailService)
        {
            _ordenDePagoService = ordenDePagoService;
            _retencionService = retencionService;
            _configuration = configuration;
            _printService = printService;
            _emailService = emailService;
            _rutaReporte = string.IsNullOrEmpty(_configuration.GetValue<string>("Parametros:RutaReporte")) ? @$"C:\Cinet\Profit\OPA" : _configuration.GetValue<string>("Parametros:RutaReporte");
        }

        public async Task<ReporteGeneradoDto> GenerarReporteOrdenDePago(OrdenDePagoModel ordenDePago, string? baseEmpresa = null)
        {
            _numeroComprobante = ordenDePago.NumeroComprobanteOPA;
            _codComprobante = ordenDePago.CodigoComprobante;
            _codProveedor = ordenDePago.CodigoProveedor;

            string rutaCarpetaRaiz = @$"{_rutaReporte}";
            if (!Directory.Exists(rutaCarpetaRaiz)) Directory.CreateDirectory(rutaCarpetaRaiz);

            string rutaArchivo = @$"{_rutaReporte}\{_codComprobante}{_numeroComprobante}.pdf";
            string rataCarpeta = @$"{_rutaReporte}\{_codComprobante}{_numeroComprobante}";

            if (!Directory.Exists(rataCarpeta)) Directory.CreateDirectory(rataCarpeta);

            var stream = new MemoryStream();
            try
            {
                using (var writer = new PdfWriter(stream))
                {
                    writer.SetCloseStream(false);
                    using (var pdf = new PdfDocument(writer))

                    /* doc es el objeto de tipo Document que se le pasa un objeto del tipo PdfDocument 
                     Con doc, se inicializa la primera página del documento */
                    using (var doc = new Document(pdf, PageSize.A4, false))
                    {
                        doc.SetMargins(110, 15, 240, 5); /* Se setea el margen del documento (pdf) */

                        /* Se obtiene la imagen y fuente del proyecto compilado */
                        byte[] rutaImagenLogo;

                        var rutaImagenLogoConfig = _configuration["Parametros:RutaImagenLogo"];

                        if (!string.IsNullOrWhiteSpace(rutaImagenLogoConfig) && File.Exists(rutaImagenLogoConfig))
                        {
                            rutaImagenLogo = await File.ReadAllBytesAsync(rutaImagenLogoConfig);
                        }
                        else
                        {
                            rutaImagenLogo = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Images.LogoMostaza.bmp");
                        }

                        var rutaFont = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.TYPEWR_B.TTF");
                        var fontCourierNewRegular = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.CourierNewRegular.ttf");

                        /* Guardo las fuentes en una lista */
                        List<byte[]> fonts = new List<byte[]> { rutaFont, fontCourierNewRegular };

                        /* Evento para la creación de Encabezado y Pie de página
                        Se va a crear un encabezado y pie por cada página que se cree. */
                        pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, new EncabezadoHandler(_ordenDePagoService, ordenDePago, doc, rutaImagenLogo, fonts, baseEmpresa));

                        /* Sección de Liquidación en Pesos */
                        //Header Liquidación en Pesos
                        if(ordenDePago.Liquidaciones.Count > 0)
                        {
                            Table liquidacionHeaderTable = new Table(new float[1]).SetWidth(UnitValue.CreatePercentValue(100));
                            Cell liquidacionHeaderCell = new Cell().Add(new Paragraph("LIQUIDACION EN PESOS").SetFontSize(16).SetPaddingLeft(25));
                            liquidacionHeaderTable.AddCell(liquidacionHeaderCell);
                            doc.Add(liquidacionHeaderTable);

                            Table tablaLiquidacionCellHeader = _ordenDePagoService.TablaLiquidacionesHeader();
                            doc.Add(tablaLiquidacionCellHeader);
                            Table tablaLiquidacion = _ordenDePagoService.TablaLiquidaciones(ordenDePago);
                            doc.Add(tablaLiquidacion);
                        }

                        /* Sección de Valores */
                        //Header Valores
                        if (ordenDePago.ValoresIng.Count > 0)
                        {
                            Table valoresHeaderTable = new Table(new float[1]).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(30);
                            Cell valoresHeaderCell = new Cell().Add(new Paragraph("VALORES").SetFontSize(16).SetPaddingLeft(25));
                            valoresHeaderTable.AddCell(valoresHeaderCell);
                            doc.Add(valoresHeaderTable);

                            Table tablaValoresHeader = _ordenDePagoService.TablaValoresHeader();
                            doc.Add(tablaValoresHeader);
                            Table tablaValores = _ordenDePagoService.TablaValores(ordenDePago, true);
                            doc.Add(tablaValores);
                        }

                        //Header Valores para recibos
                        if (ordenDePago.ValoresEgr.Count > 0)
                        {
                            doc.Add(new Paragraph("").SetMarginTop(15));
                            Table tablaValores = _ordenDePagoService.TablaValores(ordenDePago, false);
                            doc.Add(tablaValores);
                        }
                    }
                }

                /* Se valida que el stream no este vacío */
                if (stream is null || stream.Length == 0)
                    throw new InvalidOperationException("No se guardó el reporte en memoria.");

                /* Se guarda el archivo */
                using (var fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream);
                }
                stream.Position = 0;

                return new ReporteGeneradoDto
                {
                    RutaArchivo = rutaArchivo,
                    StreamArchivo = stream
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar la orden de pago: {ex}");
            }
        }

        public async Task<ReporteGeneradoDto> GenerarReporteRetencion(RetencionModel retencion)
        {
            _numeroComprobante = retencion.RetencionPracticada.NumeroComprobante;
            _codComprobante = retencion.RetencionPracticada.TipoComprobante;
            _codProveedor = retencion.SujetoRetenido.CodigoProveedor;

            string rutaCarpetaRaiz = @$"{_rutaReporte}\{_codComprobante}{_numeroComprobante}\Retenciones";
            if (!Directory.Exists(rutaCarpetaRaiz)) Directory.CreateDirectory(rutaCarpetaRaiz);
            string rutaArchivo = @$"{_rutaReporte}\{_codComprobante}{_numeroComprobante}\Retenciones\Retencion_{retencion.CodigoRetencion}_{retencion.NumeroRetencion}_{retencion.CodigoConcepto}_{_codComprobante}{_numeroComprobante}.pdf";
            var stream = new MemoryStream();
            try
            {
                using (var writer = new PdfWriter(stream))
                {
                    writer.SetCloseStream(false);
                    using (var pdf = new PdfDocument(writer))
                    using (var doc = new Document(pdf, PageSize.A4, false))
                    {
                        // Tipo de reporte
                        bool reportePrincipal = true;
                        
                        // Firma
                        var rutaFirma = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Images.firma.bmp");

                        // Fuentes
                        var fuenteTNRR = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.TimesNewRomanRegular.ttf");
                        var fuenteTNRB = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.TimesNewRomanBold.ttf");
                        var fuenteNMLR = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.NimbusMonoLRegular.ttf");
                        var fuenteNMLB = await Funciones.ObtenerRecurso("CinetReportManager.Resources.Fonts.NimbusMonoLBold.ttf");
                        List<byte[]> fuentes = new List<byte[]> { fuenteTNRR, fuenteTNRB, fuenteNMLR, fuenteNMLB };

                        /* Cabecera */
                        string Retencion_Titulo = "";
                        string Nombre_Impuesto = "";
                        string Nombre_Comprobante = "";
                        string Aclaracion_Firma = retencion.Firma ?? "";

                        switch (retencion.CodigoRetencion)
                        {
                            case "IIBB":
                                Retencion_Titulo = "RETENCIONES DE INGRESOS BRUTOS DE LA PROV. DE BS.AS.";
                                Nombre_Impuesto = "Ingresos Brutos de la Prov. de Bs As.";
                                reportePrincipal = true;
                                break;
                            case "IIBBCABA":
                                Retencion_Titulo = "RETENCIONES DE INGRESOS BRUTOS CAPITAL FEDERAL";
                                Nombre_Impuesto = "Ingresos Brutos Capital Federal";
                                reportePrincipal = true;
                                break;
                            case "IBMENDOZA":
                                Retencion_Titulo = "RETENCIONES DE INGRESOS BRUTOS MENDOZA";
                                Nombre_Impuesto = "Ingresos Brutos Mendoza";
                                reportePrincipal = true;
                                break;
                            case "IIBBSFE":
                                Retencion_Titulo = "RETENCIONES DE INGRESOS BRUTOS SANTA FE";
                                Nombre_Impuesto = "Ingresos Brutos Santa Fe";
                                reportePrincipal = true;
                                break;
                            case "IVAM":
                                retencion.AgenteRetencion.CuitAgente = "30-71583994-2";
                                Retencion_Titulo = "RETENCIONES DE VALOR AGREGADO";
                                Nombre_Impuesto = "Impuesto al Valor Agregado";
                                reportePrincipal = false;
                                break;
                            case "RG830":
                                Retencion_Titulo = "RETENCIONES DE GANANCIAS";
                                Nombre_Impuesto = "Impuesto a las Ganancias";
                                reportePrincipal = false;
                                break;
                            case "RSUSS":
                                Retencion_Titulo = "RETENCIONES DE SEGURIDAD SOCIAL (SUSS)";
                                Nombre_Impuesto = "Aportes Seguridad Social (SUSS)";
                                reportePrincipal = false;
                                break;
                            default:
                                Retencion_Titulo = retencion.RetencionPracticada.TipoImpuesto ?? "";
                                Nombre_Impuesto = retencion.RetencionPracticada.DescripcionReten ?? "";
                                reportePrincipal = true;
                                break;
                        }

                        switch (retencion.RetencionPracticada.TipoComprobante)
                        {
                            case "OPA":
                                Nombre_Comprobante = "Orden de Pago";
                                break;
                            default:
                                break;
                        }

                        if (reportePrincipal)
                        {
                            doc.SetMargins(0, 15, 7, 7);
                            doc.SetFontSize(12);
                            doc.SetFont(PdfFontFactory.CreateFont(fuenteTNRR, PdfEncodings.WINANSI));
                        }
                        else
                        {
                            doc.SetMargins(0, 15, 7, 20);
                            doc.SetFontSize(12);
                            doc.SetFont(PdfFontFactory.CreateFont(fuenteNMLR, PdfEncodings.WINANSI)).SetFontSize(10);
                        }

                        pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, new FirmaHandlerReten(retencion, fuentes, rutaFirma, Aclaracion_Firma, reportePrincipal));

                        Paragraph paragraphRenTitulo = new Paragraph(Retencion_Titulo).SetMarginBottom(-5)
                            .SetFont(PdfFontFactory.CreateFont(fuenteTNRB, PdfEncodings.WINANSI));

                        if (!reportePrincipal) 
                            paragraphRenTitulo = new Paragraph("");

                        doc.Add(paragraphRenTitulo);

                        /* Datos generales (Certificado número, fecha, etc) */
                        Table tablaDatosGenerales = _retencionService.DatosGenerales(retencion, fuentes, reportePrincipal);
                        doc.Add(tablaDatosGenerales);

                        /* Datos del Agente de Retención */
                        _retencionService.DatosAgenteDeRetencion(retencion, fuentes, doc, reportePrincipal);

                        /* Datos del Sujeto Retenido */
                        _retencionService.DatosSujetoRetenido(retencion, fuentes, doc, reportePrincipal);

                        if (reportePrincipal)
                        {
                            LineSeparator lineSeparator = new LineSeparator(new SolidLine());
                            doc.Add(lineSeparator);
                        }

                        /* Datos de la retención practicada */
                        _retencionService.DatosRetencionPracticada(retencion, fuentes, doc, Nombre_Impuesto, Nombre_Comprobante, reportePrincipal);

                        /* Porcentajes a pagar */
                        if(reportePrincipal) 
                            _retencionService.PorcentajesAPagar(retencion, fuentes, doc, reportePrincipal);
                    }
                }
                using (var fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream);
                }
                stream.Position = 0;

                return new ReporteGeneradoDto
                {
                    RutaArchivo = rutaArchivo,
                    StreamArchivo = stream
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GenerarReporteResponse> GenerarReporte(LlamadoDto llamadoDto)
        {
            GenerarReporteResponse response = new GenerarReporteResponse()
            {
                Success = false,
                Message = ""
            };

            List<MemoryStream> streams = new List<MemoryStream>();
            Dictionary<string, Stream> streamsDictionary = new Dictionary<string, Stream>();
            StringBuilder sbRespuesta = new();

            try
            {
                /* Llamado al método para generar el reporte. 
                 En dicho llamado, se guarda en el nombre del archivo del diccionario declarado anteriormente, el stream del archivo generado */
                string nombreArchivoCbte = $"{llamadoDto.OrdenDePago.CodigoComprobante}_{llamadoDto.OrdenDePago.NumeroComprobanteOPA}.pdf";

                var reporteGenerado = await GenerarReporteOrdenDePago(llamadoDto.OrdenDePago, llamadoDto.BaseEmpresa);
                streamsDictionary[nombreArchivoCbte] = reporteGenerado.StreamArchivo;

                response.ArchivosGenerados.Add(reporteGenerado.RutaArchivo);

                sbRespuesta.AppendLine($"Se generó el reporte del comprobante {llamadoDto.OrdenDePago.CodigoComprobante} - {llamadoDto.OrdenDePago.NumeroComprobanteOPA}.");

                /* Generación de retenciones */
                /* Si hay retenciones, entra al if */
                if (llamadoDto.Retenciones is not null && llamadoDto.Retenciones.Any())
                {
                    try
                    {
                        foreach (var retencion in llamadoDto.Retenciones)
                        {
                            try
                            {
                                string nombreArchivoRet = $"{retencion.CodigoRetencion}_{retencion.NumeroRetencion}_{retencion.RetencionPracticada.TipoComprobante}_{retencion.RetencionPracticada.NumeroComprobante}.pdf";

                                var reporteGeneradoRet = await GenerarReporteRetencion(retencion);
                                streamsDictionary[nombreArchivoRet] = reporteGeneradoRet.StreamArchivo;

                                response.ArchivosGenerados.Add(reporteGeneradoRet.RutaArchivo);

                                sbRespuesta.AppendLine($"Se generó reporte de la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion}");
                            }
                            catch (Exception ex)
                            {
                                sbRespuesta.AppendLine($"Error en la generación de la retención {retencion.CodigoRetencion}_{retencion.NumeroRetencion}_{retencion.RetencionPracticada.TipoComprobante}_{retencion.RetencionPracticada.NumeroComprobante}. Validar. {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sbRespuesta.AppendLine($"Error en la generación de retenciones. Validar. {ex.Message}");
                    }
                }

                /* Envío de Email */
                try
                {
                    await _emailService.EnviarEmailAProveedor(llamadoDto.OrdenDePago.EmailsProveedores, llamadoDto.OrdenDePago.NumeroComprobanteOPA, streamsDictionary);
                    sbRespuesta.AppendLine("El envío del email fue correcto.");
                }
                catch (Exception ex)
                {
                    sbRespuesta.AppendLine($"Se generó el reporte, sin embargo el envío del email falló por: {ex.Message}");
                }

                response.Success = true;
                response.Message = sbRespuesta.ToString();

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Error en llamado a generación de reporte: {ex.Message}. \n{ex}";
                return response;
            }
            finally
            {
                foreach (var stream in streamsDictionary)
                {
                    stream.Value.Dispose();
                }
            }
        }

        public async Task<GenerarReporteResponse> GenerarRetenciones(List<RetencionModel> retenciones)
        {
            GenerarReporteResponse response = new GenerarReporteResponse()
            {
                Success = false,
                Message = ""
            };

            StringBuilder sb = new StringBuilder();
            int i = 0;
            try
            {
                foreach (var retencion in retenciones)
                {
                    try
                    {
                        var reporteGeneradoRet = await GenerarReporteRetencion(retencion);

                        response.ArchivosGenerados.Add(reporteGeneradoRet.RutaArchivo);

                        sb.AppendLine($"Se generó la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion}.");
                        i++;
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error al generar la retención {retencion.CodigoRetencion} - {retencion.NumeroRetencion} - {ex.Message}.");
                    }
                }
                string msg = sb.ToString();
                if (i == 0)
                {
                    throw new Exception($"No se generaron las retenciones: {msg}");
                }

                response.Success = true;
                response.Message = sb.ToString();

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Error en llamado a generación de reporte de retención: {ex.Message}. \n{ex}";
                return response;
            }
        }
    }
}
