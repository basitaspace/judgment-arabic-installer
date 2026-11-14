param(
    [Parameter(Mandatory = $true)]
    [string]$GameDataPath,

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$GameDataPath = (Resolve-Path -LiteralPath $GameDataPath).Path
$required = @(
    "db.judge.en.par",
    "font.judge\en\meta_ot_cond_book.dds",
    "font.judge\en\meta_ot_cond_book_italic.dds"
)
$optional = @("tarib_judgment.txt")

foreach ($relative in $required) {
    $source = Join-Path $GameDataPath $relative
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "Required patch file is missing: $source"
    }
}

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $privateDir = Join-Path $projectRoot "private-payload"
    New-Item -ItemType Directory -Force -Path $privateDir | Out-Null
    $OutputPath = Join-Path $privateDir "Judgment-Arabic-Patch.zip"
}
$outputFull = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFull) | Out-Null

$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("JudgmentPatchBuild_" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $stage | Out-Null

try {
    $included = @()
    foreach ($relative in ($required + $optional)) {
        $source = Join-Path $GameDataPath $relative
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            $destination = Join-Path $stage $relative
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
            Copy-Item -LiteralPath $source -Destination $destination -Force
            $included += $relative.Replace("\", "/")
        }
    }

    $info = @(
        "Judgment Arabic localization patch",
        "Package created: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
        "Source data directory: $GameDataPath",
        "This package contains only the explicitly listed modified resources."
    )
    Set-Content -LiteralPath (Join-Path $stage "PATCH_INFO.txt") -Value $info -Encoding UTF8

    $checksums = foreach ($relative in $included) {
        $source = Join-Path $stage ($relative.Replace("/", "\"))
        $hash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $relative"
    }
    Set-Content -LiteralPath (Join-Path $stage "SHA256SUMS.txt") -Value $checksums -Encoding ASCII

    if (Test-Path -LiteralPath $outputFull) { Remove-Item -LiteralPath $outputFull -Force }
    Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $outputFull -CompressionLevel Optimal

    Write-Host "Patch package created: $outputFull" -ForegroundColor Green
    Write-Host "Included files:" -ForegroundColor Cyan
    $included | ForEach-Object { Write-Host " - $_" }
}
finally {
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
}
