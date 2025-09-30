# Sign NuGet Package Script
# This script signs the TileCanvas NuGet package using the CodingConnected code signing certificate

param(
    [Parameter(Mandatory=$false)]
    [string]$PackagePath = "",
    [Parameter(Mandatory=$false)]
    [string]$OutputDirectory = "",
    [Parameter(Mandatory=$false)]
    [switch]$Overwrite
)

# Certificate details - auto-detect by subject name
$CertificateSubjectName = "CodingConnected e.U."
$TimestampServer = "http://timestamp.sectigo.com"  # Required for NuGet packages

# Find package if not specified
if ([string]::IsNullOrEmpty($PackagePath)) {
    $PackagePath = Get-ChildItem -Recurse -Filter "*.nupkg" | 
                   Where-Object { $_.Name -like "CodingConnected.WPF.TileCanvas.*" } | 
                   Select-Object -First 1 -ExpandProperty FullName
    
    if ([string]::IsNullOrEmpty($PackagePath)) {
        Write-Error "No package found. Please build the package first or specify -PackagePath"
        exit 1
    }
}

Write-Host "Package to sign: $PackagePath" -ForegroundColor Green
Write-Host "Signing method: Store" -ForegroundColor Green

Write-Host "Searching for certificate: $CertificateSubjectName" -ForegroundColor Green

# Find certificate by subject name (auto-detect thumbprint)
$cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { 
    $_.Subject -like "*$CertificateSubjectName*" -and 
    $_.HasPrivateKey -and 
    $_.NotAfter -gt (Get-Date) 
} | Sort-Object NotAfter -Descending | Select-Object -First 1

if (-not $cert) {
    Write-Error "No valid certificate found with subject name '$CertificateSubjectName' in CurrentUser\My store"
    Write-Host "Available certificates:" -ForegroundColor Yellow
    Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.HasPrivateKey } | ForEach-Object {
        Write-Host "  - $($_.Subject) (expires: $($_.NotAfter))" -ForegroundColor Gray
    }
    exit 1
}

$CertificateThumbprint = $cert.Thumbprint
Write-Host "Found certificate: $($cert.Subject)" -ForegroundColor Green
Write-Host "Thumbprint: $CertificateThumbprint" -ForegroundColor Green
Write-Host "Valid until: $($cert.NotAfter)" -ForegroundColor Green

# Build nuget sign command based on method
$signArgs = @(
    "sign"
    "`"$PackagePath`""
    "-Timestamper"
    $TimestampServer
    "-HashAlgorithm"
    "SHA256"
    "-TimestampHashAlgorithm"
    "SHA256"
    "-Verbosity"
    "detailed"
)

$signArgs += @(
    "-CertificateFingerprint"
    $CertificateThumbprint
)

if (-not [string]::IsNullOrEmpty($OutputDirectory)) {
    $signArgs += @("-OutputDirectory", "`"$OutputDirectory`"")
}

if ($Overwrite) {
    $signArgs += "-Overwrite"
}

# Execute signing
Write-Host "Executing: nuget $($signArgs -join ' ')" -ForegroundColor Yellow
Write-Host ""

try {
    & nuget $signArgs
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Package signed successfully!" -ForegroundColor Green
        
        # Verify the signature
        Write-Host "Verifying signature..." -ForegroundColor Yellow
        $verifyResult = & nuget verify -Signatures $PackagePath 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Signature verification successful!" -ForegroundColor Green
        } else {
            Write-Host "Signature verification had issues:" -ForegroundColor Yellow
            Write-Host $verifyResult
        }
    } else {
        Write-Host "Package signing failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "Error signing package: $_" -ForegroundColor Red
    exit 1
}