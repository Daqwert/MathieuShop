param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$Host,
    [int]$Port,
    [string]$Database,
    [string]$Username,
    [string]$Password
)

$ErrorActionPreference = "Stop"

Set-Location $ProjectRoot

Write-Host "MathieuShop: настройка PostgreSQL..." -ForegroundColor Cyan
Write-Host "Project root: $ProjectRoot"
Write-Host "Будет использован appsettings.json из MathieuShop.Avalonia"

$configPath = Join-Path $ProjectRoot "MathieuShop.Avalonia\appsettings.json"

if ($PSBoundParameters.ContainsKey("Host") -or
    $PSBoundParameters.ContainsKey("Port") -or
    $PSBoundParameters.ContainsKey("Database") -or
    $PSBoundParameters.ContainsKey("Username") -or
    $PSBoundParameters.ContainsKey("Password")) {

    $config = Get-Content $configPath -Raw | ConvertFrom-Json

    $builder = @{
        Host = if ($Host) { $Host } else { "localhost" }
        Port = if ($Port) { $Port } else { 5432 }
        Database = if ($Database) { $Database } else { "mathieu_shop" }
        Username = if ($Username) { $Username } else { "postgres" }
        Password = if ($Password) { $Password } else { "postgres" }
    }

    $config.ConnectionStrings.PostgreSql = "Host=$($builder.Host);Port=$($builder.Port);Database=$($builder.Database);Username=$($builder.Username);Password=$($builder.Password)"
    $config | ConvertTo-Json -Depth 10 | Set-Content -Encoding utf8 $configPath

    Write-Host "Строка подключения обновлена в appsettings.json" -ForegroundColor Yellow
}

dotnet tool restore
dotnet run --project ".\MathieuShop.DatabaseSetup\MathieuShop.DatabaseSetup.csproj" -- "$ProjectRoot"

Write-Host ""
Write-Host "Готово. Теперь можно запускать приложение." -ForegroundColor Green
