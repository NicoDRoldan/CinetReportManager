using AvisoReporte.Models.Comprobante;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvisoReporte.Models.DTO
{
    public class ComprobanteDto
    {
        public static string Nro_Comprobante { get; set; }
        public static string Cod_Comprobante { get; set; }
        public static string Nro_Sucursal { get; set; }
        public static string Cod_Proveedor { get; set; }

        public static async Task ObtenerClavesDeComprobante(string claves)
        {
            int index = -1;
            for (int i = 0; i < claves.Length; i++)
            {
                if (char.IsLetter(claves[i]))
                {
                    index = i;
                    break;
                }
            }
            int indexTotal = -1;
            for (int i = index; i < claves.Length; i++)
            {
                if (!char.IsLetter(claves[i]))
                {
                    indexTotal = i;
                    break;
                }
            }

            Cod_Comprobante = claves.Substring(index, indexTotal - index);
            Nro_Comprobante = claves.Substring(indexTotal + 4);
            Nro_Sucursal = claves.Substring(indexTotal, 4);
            Cod_Proveedor = claves.Substring(0, index);
        }
    }
}
