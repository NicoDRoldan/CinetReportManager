using CinetReportManager.Models;
using iText.Layout.Element;

namespace CinetReportManager.Interfaces
{
    public interface IOrdenDePagoService
    {
        Table TablaEncabezado(OrdenDePagoModel ordenDePago, byte[] rutaImagen, byte[] rutaFont);
        Table TablaEncabezado(OrdenDePagoModel ordenDePago, List<byte[]> rutaFont, string baseEmpresa);
        Table TablaProveedor(OrdenDePagoModel ordenDePago);
        Table TablaLiquidacionesHeader();
        Table TablaLiquidaciones(OrdenDePagoModel ordenDePago);
        Table TablaValoresHeader();
        Table TablaValores(OrdenDePagoModel ordenDePago, bool inge_numero);
    }
}
