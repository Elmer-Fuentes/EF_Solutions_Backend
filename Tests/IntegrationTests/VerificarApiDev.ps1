# Autor: Elmer Fuentes
# Comprueba el contrato HTTP de la API de desarrollo en una instancia local.
[CmdletBinding()]
param([ValidateRange(1, 65535)][int]$Puerto = 5257)

$ErrorActionPreference = 'Stop'
$base = "http://127.0.0.1:$Puerto"

$estado = Invoke-RestMethod "$base/api/v1/plataforma/estado" -TimeoutSec 5
if ($estado.sistema -ne 'EF Solutions' -or $estado.estado -ne 'disponible') {
    throw 'El contrato de estado de plataforma cambió.'
}

$swagger = Invoke-RestMethod "$base/swagger/v1/swagger.json" -TimeoutSec 5
foreach ($ruta in '/health', '/health/database', '/api/v1/plataforma/estado') {
    if (-not $swagger.paths.PSObject.Properties[$ruta].Value.get) {
        throw "Falta GET $ruta en Swagger."
    }
}

try {
    $respuesta = Invoke-WebRequest "$base/health/database" -UseBasicParsing -TimeoutSec 10
    throw "El diagnóstico respondió HTTP $($respuesta.StatusCode) con MySQL inaccesible."
}
catch {
    if (-not $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne 503) {
        throw
    }
}

Write-Output 'Contrato de plataforma y Swagger verificados. MySQL inaccesible devuelve HTTP 503.'
