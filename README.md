# Backend — solucion unica .NET

**Autor:** Elmer Fuentes.

La solucion `EF.Solutions.slnx` contiene **exactamente tres proyectos**: `EF.Solutions.Web`, `EF.Solutions.Business` y `EF.Solutions.Data`. Los archivos se organizan por funcionalidad dentro de las capas.

- `Web`: Controllers/<Modulo>, middleware, HTTP y composicion.
- `Business`: Services/<Modulo>, DTOs/<Modulo>, Interfaces/<Modulo>, Validators/<Modulo>.
- `Data`: Repositories/<Modulo>, Connections/<Proveedor>, Providers.

Referencias: `Web -> Business`, `Web -> Data` para registrar dependencias; `Data -> Business`. `Business` no depende de `Data` ni `Web`.

Configuraciones de ejemplo: `Configuracion/env/`. El `.env` de trabajo es `EF.Solutions.Web/.env` (no versionar).

Pruebas futuras: `Tests/UnitTests`, `Tests/IntegrationTests`, `Tests/ArchitectureTests`.

Para ejecutar:

```powershell
dotnet restore .\EF.Solutions.slnx
dotnet build .\EF.Solutions.slnx
```

No afirmamos que compile hasta ejecutarlo en ambiente con SDK .NET 10.
