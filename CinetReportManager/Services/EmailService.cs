using CinetReportManager.Helpers;
using CinetReportManager.Interfaces;
using CinetReportManager.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;

namespace CinetReportManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailModel _emailModel;
        private readonly ITemplateService _templateService;

        public EmailService(IConfiguration configuration, EmailModel emailModel, ITemplateService templateService)
        {
            _configuration = configuration;
            _emailModel = emailModel;
            _templateService = templateService;
        }

        public async Task EnviarEmailAProveedor(List<string> emailsProveedores, string nroComprobanteOpa, Dictionary<string, Stream> streamsDictionary)
        {
            _emailModel.ObtenerConfiguracion();

            if (!string.IsNullOrEmpty(_emailModel.ReceiveTest))
            {
                emailsProveedores.Clear();
                emailsProveedores.Add(_emailModel.ReceiveTest);
            }

            if (!ValidarEmails(emailsProveedores)) throw new Exception("Se generó el reporte pero no hay emails para hacer el envío.");

            MemoryStream? msImagen = null;

            try
            {
                /* Configuración del cliente para el envío */
                SmtpClient smtpClient = ConfigurarSmtp();

                // Se instancia el objeto MailMessage
                var message = new MailMessage();

                // Email que envía
                message.From = new MailAddress(_emailModel.Email_Desde);

                // Destinatarios
                foreach (var email in emailsProveedores)
                {
                    message.To.Add(email);
                }

                // Asunto
                message.Subject = $"Orden de pago {nroComprobanteOpa}";

                // Convertir Body en HTML
                message.IsBodyHtml = true;
                string bodyHtml = _configuration["Parametros:RutaHtml"] ?? "";

                if (!string.IsNullOrEmpty(bodyHtml) && File.Exists(bodyHtml))
                {
                    bodyHtml = File.ReadAllText(bodyHtml);

                    Dictionary<string, string> placeholders = new()
                    {
                        { "nroComprobanteOpa", nroComprobanteOpa }
                    };

                    bodyHtml = await _templateService.ApplyPlaceholdersAsync(bodyHtml, placeholders);
                }
                else
                {
                    bodyHtml = $@"
                    <html>
                    <body>
                        <h1>Orden de pago {nroComprobanteOpa}</h1>
                        <p>MOSTAZA Y PAN S.A.</p>
                        <p>Av Ing Huergo 953 P.11 CABA C1107AOJ</p>
                        <p>Teléfono: 3754-2200</p>
                        <img src='cid:mostaza-logo' alt='Imagen' style='width:100px; height:80px;'>
                    </html>
                    </body>";
                }

                var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, "text/html");

                // Agregar imagen al HTML

                var rutaImagen = _configuration["Parametros:RutaImagenLogo"] ?? "";

                if (!string.IsNullOrEmpty(rutaImagen) && File.Exists(rutaImagen))
                {
                    rutaImagen = rutaImagen.Trim()
                            .Replace("\u200E", "") // LRM
                            .Replace("\u200F", "") // RLM
                            .Replace("\u202A", "") // LRE
                            .Replace("\u202B", "") // RLE
                            .Replace("\u202C", "") // PDF
                            .Replace("\u202D", "") // LRO
                            .Replace("\u202E", "") // RLO
                            .Replace("\u2066", "") // LRI
                            .Replace("\u2067", "") // RLI
                            .Replace("\u2068", "") // FSI
                            .Replace("\u2069", "") // PDI
                            .Replace("\uFEFF", ""); // BOM / ZWNBSP

                    var imagen = new LinkedResource(rutaImagen)
                    {
                        ContentId = "mostaza-logo",
                        TransferEncoding = TransferEncoding.Base64,
                        ContentType =
                            {
                                Name = $"image/{Path.GetExtension(rutaImagen).ToLowerInvariant()}"
                            }
                    };
                    htmlView.LinkedResources.Add(imagen);
                }
                else
                {
                    using var streamImagen = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("CinetReportManager.Resources.Images.LogoMostaza.bmp")
                        ?? throw new Exception("Error al cargar el recurso");

                    msImagen = new MemoryStream();
                    streamImagen.CopyTo(msImagen);
                    msImagen.Position = 0;

                    var lr = new LinkedResource(msImagen)
                    {
                        ContentId = "mostaza-logo",
                        TransferEncoding = TransferEncoding.Base64,
                    };
                    lr.ContentType.MediaType = "image/bmp";
                    htmlView.LinkedResources.Add(lr);
                }

                message.AlternateViews.Add(htmlView);

                // Agregar archivos adjuntos
                foreach (var dic in streamsDictionary)
                {
                    var nombreArchivo = dic.Key;
                    var stream = dic.Value;
                    var adjunto = new Attachment(stream, nombreArchivo);
                    message.Attachments.Add(adjunto);
                }

                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en envío de email: {ex.Message}");
            }
            finally
            {
                msImagen?.Dispose();
            }
        }

        public SmtpClient ConfigurarSmtp()
        {
            /* Configuración del cliente para el envío */
            SmtpClient smtpClient = new SmtpClient(_emailModel.Servicio_Email);

            smtpClient.UseDefaultCredentials = false;
            smtpClient.Port = _emailModel.PuertoEmail;
            smtpClient.Credentials = new NetworkCredential(_emailModel.Email_Desde, _emailModel.Clave_Email);
            smtpClient.EnableSsl = _emailModel.EnableSsl;

            return smtpClient;
        }

        public bool ValidarEmails(List<string> Emails)
        {
            foreach (var email in Emails)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
