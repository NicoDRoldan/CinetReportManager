# CinetReportManager
**CinetReportManager** es una **API** la cual es ejecutada cómo un servicio de Windows.
El proyecto está hecho con **.NET Core 6**.
> [!NOTE]
> El proyecto se complementa junto a otro `AvisoReporte`. Este es un ejecutable el cual se encarga de buscar la información de los comprobantes en la base de datos y enviar en formato json dicha información a la API CinetReportManager.
## Flujo Principal
1. Recibir información del comprobante en formato Json a través de una solicitud `POST` al EndPoint `api/GenerarReporte`.
2. Con los datos recibidos, se genera la orden de pago.
   - Si hay retenciones asociadas, se generarán también sus respectivos reportes.
3. Rutas donde se guarda la orden de pago (sujetos a cambio):
   - Ruta de orden de pago: ```C:\Cinet\Profit\OPA_{egre_numero}\OPA.PDF.```
   - Ruta de retenciones: ```C:\Cinet\Profit\OPA_{egre_numero}\Retenciones\Retencion.PDF.```
4. Se envían los reportes correspondientes al email o emails asociados al proveedor.
> Los datos del `email` se sacan de la columna `PRO_EMAIL` de la tabla Proveedores.

## Tipo de reportes
### Reporte de Orden de Pago.
- Se utiliza un solo tipo de reporte.
### Reporte de Retenciones: 
- Existen dos tipos de reportes, dependiendo del tipo de retención:.
1. **Principales**: IIBB, IIBB CABA, IIBB Mendoza.
2. **Secundarios**: IVA M, RG830.

## Formato Json que recibe el EndPoint `api/GenerarReporte`
``` Json
{
  "OrdenDePago": {
    "CodigoComprobante": "",
    "NumeroComprobanteOPA": "",
    "CodigoSucursal": "",
    "FechaOPA": "",
    "CodigoProveedor": "",
    "RazonSocialProveedor": "",
    "EmailsProveedores": [],
    "Liquidaciones": [
      {
        "CodigoComprobante": "",
        "NumeroComprobanteFAC": "",
        "FechaFactura": "",
        "Saldo": 0.0,
        "ImportePagado": 0.0
      }
    ],
    "ValoresIng": [
      {
        "FechaValor": "",
        "Descripcion": "",
        "NumMovimiento": "",
        "Importe": 0.0
      }
    ],
    "ValoresEgr":  [
      {
        "FechaValor": "",
        "Descripcion": "",
        "NumMovimiento": "",
        "Importe": 0.0
      }
    ]
  },
  "Retenciones": [
    {
      "NumeroRetencion": "",
      "CodigoRetencion": "",
      "Fecha": "",
      "IB": "",
      "AgenteRetencion": {
        "Denominacion": "",
        "DireccionAgente": "",
        "IvaAgente": "",
        "CuitAgente": ""
      },
      "SujetoRetenido": {
        "RazonSocialSujeto": "",
        "CuitSujeto": "",
        "DireccionRetenido": ""
      },
      "RetencionPracticada": {
        "TipoImpuesto": "",
        "TipoComprobante": "",
        "NumeroComprobante": "",
        "DescripcionReten": "",
        "ImporteOriginaReten": 0.0
      },
      "BaseImponible": 0.0,
      "Porcentaje": 0.0,
      "ImporteRetencion": 0.0,
      "Firma": ""
    }
  ]
}

```

## Archivo de Configuración `AppSettings.Json`
> El archivo de configuración tiene los siguientes parámetros:
``` Json
{
  "Parametros": {
    "PuertoConfig": 7253 // Puerto en el que se ejecutará la API
  },
  "EmailConfig": {
    "Email": "", // Email que enviará el correo
    "PassEmail": "", // Contraseña y/o autenticación del correo que envía el email
    "ServerEmail": "smtp.gmail.com", //  Servidor SMTP
    "PuetoEmail": 587, // Puerto SMTP
    "EnableSsl": true, // Seguridad SSL
    "ReceiveTest":  "" // Si este campo existe y no está vacío, se enviará el email solo a este correo
  }
}
```

# AvisoReporte
**AvisoReporte** es una aplicación de consola que envía datos a **CinetReportManager** cuando se genera un comprobante en `Profit`.
El proyecto está hecho con **.NET Core 6**.
## Flujo Principal
1. Recibir notificación de **`Profit`** al generarse un comprobante.
2. Buscar toda la información del comprobante en base de datos.
3. Enviar datos del comprobante a **`CinetReportManager`**.

## Formato de los datos enviados por Profit:
Ejemplo `009840OPA999900000001`
- **009840**: Código de Proveedor.
- **OPA**: Código de Comprobante.
- **9999**: Número de Sucursal.
- **00000001**: Número de Comprobante.

## Conexión a base de datos
La conexión se realiza usando `OdbcConnection`. Con datos de configuración en un archivo `.Config`.
- **TipoConexion**: Define el tipo de contraseña a utilizar (1 para local, 2 para central).
- **Empresa**: Indica a que base de datos se conectará.

## Archivo de Configuración `.Config`
``` Xml
<configuration>
	<appSettings>
<!-- Puerto donde hará la llamada a la API CinetReportManager -->
		<add key="PuertoReportManager" value="7253"/> 
 <!-- 1: Contraseña Local / 2: Contraseña Central -->
		<add key="TipoConexion" value="2"/>
<!-- 1: Backoffice; 2: Cinet_PDV: 3: Mostaza_ERP; 4: FRQ_ERP; 5: PROSPEROUS_ERP; 6: DAFIRUZ_ERP; 100: TEST_ERP -->
		<add key="Empresa" value="100"/>
	</appSettings>
</configuration>
```
