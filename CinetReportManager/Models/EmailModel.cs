using Microsoft.Extensions.Configuration;

namespace CinetReportManager.Models
{
    public class EmailModel
    {
        public string Email_Desde { get; set; }
        public string Clave_Email { get; set; }
        public string Servicio_Email { get; set; }
        public int PuertoEmail { get; set; }
        public bool EnableSsl { get; set; }
        public string? ReceiveTest { get; set; }

        private readonly IConfiguration _configuration;

        public EmailModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ObtenerConfiguracion()
        {
            Email_Desde = _configuration.GetValue<string>("EmailConfig:Email")!;
            Clave_Email = _configuration.GetValue<string>("EmailConfig:PassEmail")!;
            Servicio_Email = _configuration.GetValue<string>("EmailConfig:ServerEmail")!;
            PuertoEmail = _configuration.GetValue<int>("EmailConfig:PuertoEmail")!;
            EnableSsl = _configuration.GetValue<bool>("EmailConfig:EnableSsl")!;
            ReceiveTest = _configuration.GetValue<string>("EmailConfig:ReceiveTest")!;
        }
    }
}
