using CinetReportManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinetReportManager.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAProveedor(List<string> emailsProveedores, string nroComprobanteOpa, Dictionary<string, Stream> streamsDictionary);
    }
}
