using CinetReportManager.Helpers;
using CinetReportManager.Interfaces;
using CinetReportManager.Models;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;

namespace CinetReportManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService (IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string Email_Desde { get; set; }
        private string Clave_Email { get; set; }
        private string Servicio_Email { get; set; }
        private int PuertoEmail { get; set; }
        private bool EnableSsl { get; set; }
        private string? ReceiveTest { get; set; }

        public async Task EnviarEmailAProveedor(List<string> emailsProveedores, string nroComprobanteOpa, Dictionary<string, Stream> streamsDictionary)
        {
            // Obtener datos del archivo de configuración
            Email_Desde = _configuration.GetValue<string>("EmailConfig:Email")!;
            Clave_Email = _configuration.GetValue<string>("EmailConfig:PassEmail")!;
            Servicio_Email = _configuration.GetValue<string>("EmailConfig:ServerEmail")!;
            PuertoEmail = _configuration.GetValue<int>("EmailConfig:PuetoEmail")!;
            EnableSsl = _configuration.GetValue<bool>("EmailConfig:EnableSsl")!;
            ReceiveTest = _configuration.GetValue<string>("EmailConfig:ReceiveTest")!;

            if (!string.IsNullOrEmpty(ReceiveTest))
            {
                emailsProveedores.Clear();
                emailsProveedores.Add(ReceiveTest);
            }

            if (!ValidarEmails(emailsProveedores)) throw new Exception("Se generó el reporte pero no hay emails para hacer el envío.");

            try
            {
                /* Configuración del cliente para el envío */
                SmtpClient smtpClient = new SmtpClient(Servicio_Email);

                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = PuertoEmail;
                smtpClient.Credentials = new NetworkCredential(Email_Desde, Clave_Email);
                smtpClient.EnableSsl = EnableSsl;

                // Se instancia el objeto MailMessage
                var message = new MailMessage();

                // Email que envía
                message.From = new MailAddress(Email_Desde);

                // Destinatarios
                foreach (var email in emailsProveedores)
                {
                    message.To.Add(email);
                }

                // Asunto
                message.Subject = $"Orden de pago {nroComprobanteOpa}";

                // Convertir Body en HTML
                message.IsBodyHtml = true;
                string bodyHtml = $@"
                    <html>
                    <body>
                        <h1>Orden de pago {nroComprobanteOpa}</h1>
                        <p>MOSTAZA Y PAN S.A.</p>
                        <p>Av Ing Huergo 953 P.11 CABA C1107AOJ</p>
                        <p>Teléfono: 3754-2200</p>
                        <img src='cid:mostaza-logo' alt='Imagen' style='width:100px; height:80px;'>
                    </html>
                    </body>";

                // Agregar imagen al HTML
                using Stream streamImagen = Assembly.GetExecutingAssembly().GetManifestResourceStream("CinetReportManager.Resources.Images.LogoMostaza.bmp") ?? throw new Exception("Error la cargar el recurso");
                using MemoryStream rutaImagen = new MemoryStream();
                streamImagen.CopyTo(rutaImagen);
                rutaImagen.Position = 0;
                var imagen = new LinkedResource(rutaImagen, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "mostaza-logo"
                };
                var avHtml = AlternateView.CreateAlternateViewFromString(bodyHtml, null, MediaTypeNames.Text.Html);
                avHtml.LinkedResources.Add(imagen);
                message.AlternateViews.Add(avHtml);

               // Agregar archivos adjuntos
                foreach(var dic in streamsDictionary)
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
                throw new Exception($"Error al enviar el email: {ex.Message}");
            }
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
