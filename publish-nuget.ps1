# NuGet Package Publishing Script
# Automates building, signing, and publishing the CodingConnected.WPF.TileCanvas package

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "",

    [Parameter(Mandatory=$false)]
    [switch]$SkipSigning = $false,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false,

    [Parameter(Mandatory=$false)]
    [string]$Source = "https://api.nuget.org/v3/index.json",

    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectPath = "src\CodingConnected.WPF.TileCanvas.Library\CodingConnected.WPF.TileCanvas.Library.csproj"
$PackageId = "CodingConnected.WPF.TileCanvas"

Write-Host "NuGet Publishing Workflow for $PackageId" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan

# Step 1: Validate inputs and environment
Write-Host ""
Write-Host "Step 1: Validation" -ForegroundColor Yellow

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project file not found: $ProjectPath"
}

# Get current version from project file
$projectXml = [xml](Get-Content $ProjectPath)
$currentVersion = $projectXml.Project.PropertyGroup.PackageVersion
Write-Host "Current version in project: $currentVersion" -ForegroundColor Green

if ([string]::IsNullOrEmpty($Version)) {
    $Version = $currentVersion
    Write-Host "Using current project version: $Version" -ForegroundColor Green
} else {
    Write-Host "Target version specified: $Version" -ForegroundColor Green
    
    # Update project file version
    $projectXml.Project.PropertyGroup.PackageVersion = $Version
    $projectXml.Project.PropertyGroup.AssemblyVersion = "$Version.0"
    $projectXml.Project.PropertyGroup.FileVersion = "$Version.0"
    $projectXml.Save((Resolve-Path $ProjectPath).Path)
    Write-Host "Project file updated with new version" -ForegroundColor Green
}

# Step 2: Clean and build
Write-Host ""
Write-Host "Step 2: Clean Build" -ForegroundColor Yellow

Write-Host "Cleaning previous builds..." -ForegroundColor Gray
dotnet clean $ProjectPath --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

Write-Host "Building project..." -ForegroundColor Gray
dotnet build $ProjectPath --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

Write-Host "Creating package..." -ForegroundColor Gray
dotnet pack $ProjectPath --configuration Release --no-build --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Pack failed" }

Write-Host "Build completed successfully" -ForegroundColor Green

# Step 3: Find the generated package
Write-Host ""
Write-Host "Step 3: Package Location" -ForegroundColor Yellow

$packagePath = Get-ChildItem -Recurse -Filter "$PackageId.$Version.nupkg" | Select-Object -First 1 -ExpandProperty FullName
if ([string]::IsNullOrEmpty($packagePath)) {
    Write-Error "Package not found: $PackageId.$Version.nupkg"
}

Write-Host "Package created: $packagePath" -ForegroundColor Green
$packageSize = [math]::Round((Get-Item $packagePath).Length / 1KB, 2)
Write-Host "Package size: $packageSize KB" -ForegroundColor Green

# Step 4: Sign package (if not skipped)
if (-not $SkipSigning) {
    Write-Host ""
    Write-Host "Step 4: Package Signing" -ForegroundColor Yellow
    
    if (Test-Path ".\sign-package.ps1") {
        Write-Host "Signing package with code signing certificate..." -ForegroundColor Gray
        & .\sign-package.ps1 -PackagePath $packagePath -Overwrite
        if ($LASTEXITCODE -ne 0) { 
            Write-Warning "Package signing failed, but continuing..."
        } else {
            Write-Host "Package signed successfully" -ForegroundColor Green
        }
    } else {
        Write-Warning "Signing script not found, skipping signing"
    }
} else {
    Write-Host ""
    Write-Host "Step 4: Skipping package signing (as requested)" -ForegroundColor Yellow
}

# Step 5: Check if package already exists on NuGet.org
Write-Host ""
Write-Host "Step 5: Version Check" -ForegroundColor Yellow

try {
    $existingVersions = & nuget list $PackageId -Source $Source -AllVersions 2>$null
    if ($existingVersions -like "*$PackageId*$Version*") {
        if (-not $Force) {
            Write-Error "Version $Version already exists on NuGet.org. Use -Force to overwrite (not recommended for public packages)"
        } else {
            Write-Warning "Version $Version already exists, but -Force specified"
        }
    } else {
        Write-Host "Version $Version is available" -ForegroundColor Green
    }
} catch {
    Write-Host "Could not check existing versions (first publish?)" -ForegroundColor Gray
}

# Step 6: Publish (or dry run)
Write-Host ""
if ($DryRun) {
    Write-Host "Step 6: Dry Run - Would Publish" -ForegroundColor Yellow
    Write-Host "Package: $packagePath" -ForegroundColor Gray
    Write-Host "Version: $Version" -ForegroundColor Gray
    Write-Host "Target: $Source" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Dry run completed - package ready for publishing!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To publish for real, run:" -ForegroundColor Cyan
    Write-Host ".\publish-nuget.ps1" -ForegroundColor White
} else {
    Write-Host "Step 6: Publishing to NuGet.org" -ForegroundColor Yellow

    Write-Host ""
    Write-Host "Please enter your NuGet API key:" -ForegroundColor Cyan
    Write-Host "(Get your API key from https://www.nuget.org/account/apikeys)" -ForegroundColor Gray
    $ApiKey = Read-Host -AsSecureString
    $ApiKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($ApiKey))

    if ([string]::IsNullOrEmpty($ApiKey)) {
        Write-Host ""
        Write-Error "API Key is required for publishing"
    }
    
    Write-Host "Publishing package to NuGet.org..." -ForegroundColor Gray
    Write-Host "This may take a few minutes..." -ForegroundColor Gray
    
    $publishArgs = @(
        "push"
        "`"$packagePath`""
        "-ApiKey"
        $ApiKey
        "-Source" 
        $Source
        "-Verbosity"
        "detailed"
    )
    
    if ($Force) {
        $publishArgs += "-SkipDuplicate"
    }
    
    & nuget $publishArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS! Package published to NuGet.org" -ForegroundColor Green
        Write-Host ""
        Write-Host "Package Details:" -ForegroundColor Cyan
        Write-Host "• ID: $PackageId" -ForegroundColor White
        Write-Host "• Version: $Version" -ForegroundColor White
        Write-Host "• Size: $packageSize KB" -ForegroundColor White
        Write-Host ""
        Write-Host "Package URL: https://www.nuget.org/packages/$PackageId/$Version" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Users can now install with:" -ForegroundColor Cyan
        Write-Host "Install-Package $PackageId" -ForegroundColor White
        Write-Host "# or" -ForegroundColor Gray
        Write-Host "dotnet add package $PackageId" -ForegroundColor White
        
    } else {
        Write-Error "Package publishing failed with exit code $LASTEXITCODE"
    }
}

Write-Host ""
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "Publishing workflow completed!" -ForegroundColor Cyan