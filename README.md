# Backend — solucion unica .NET

**Autor:** Elmer Fuentes.

La solucion `EF.Solutions.slnx` contiene **exactamente tres proyectos**: `EF.Solutions.Web`, `EF.Solutions.Business` y `EF.Solutions.Data`. Los archivos se organizan por funcionalidad dentro de las capas.

- `Web`: Controllers/<Modulo>, middleware, HTTP y composicion.
- `Business`: Services/<Modulo>, DTOs/<Modulo>, Interfaces/<Modulo>, Validators/<Modulo>.
- `Data`: Repositories/<Modulo>, Connections/<Proveedor>, Providers.

Referencias: `Web -> Business`, `Web -> Data` para registrar dependencias; `Data -> Business`. `Business` no depende de `Data` ni `Web`.

Configuraciones de ejemplo: `Configuracion/env/`. El `.env` de trabajo es `EF.Solutions.Web/.env` (no versionar).

- **Development:** usa `.env.local.example` o `EF.Solutions.Web/.env.example` como referencia. `EF_CONFIG_FILE` permite señalar otro archivo privado de DEV. Las variables del proceso tienen prioridad.
- **QA:** inicia con `DOTNET_ENVIRONMENT=Staging` y variables protegidas independientes.
- **Producción:** usa las claves de `.env.production.example` como referencia para Railway. El servicio recibe las variables protegidas y conecta con TLS `VerifyFull`. Los perfiles externos requieren bases, identidades y credenciales independientes.

### MySQL DEV opcional con Docker

Desde `Backend/`, define `EF_MYSQL_ROOT_PASSWORD` en tu sesión privada y ejecuta `docker compose -f Configuracion/compose.mysql-dev.yml up -d`. La instancia usa un volumen propio y escucha en `127.0.0.1:3307`. Aprovisiona `ef_dev` antes de conectar la API y configura su puerto en el archivo privado. Para detener la instancia y conservar sus datos, usa `docker compose -f Configuracion/compose.mysql-dev.yml down`.

Pruebas futuras: `Tests/UnitTests`, `Tests/IntegrationTests`, `Tests/ArchitectureTests`.

Para ejecutar en desarrollo con MySQL local activo y `EF.Solutions.Web/.env` configurado, abre PowerShell:

```powershell
Set-Location 'C:\DEV\Leonel\EF_Solutions\Backend\EF.Solutions.Web'
dotnet run
```

En otra terminal, consulta `http://localhost:5257/health` para verificar la API y `http://localhost:5257/health/database` para confirmar la conexión a `EF_DEV`. Detén la API con Ctrl+C en su terminal.

En Development, abre [Swagger UI](http://localhost:5257/swagger/) para elegir una operación y ejecutarla con **Try it out** y **Execute**. El contrato OpenAPI está en `/swagger/v1/swagger.json`.
