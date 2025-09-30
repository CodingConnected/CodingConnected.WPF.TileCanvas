# NuGet Publishing Guide

This guide walks you through publishing your CodingConnected.WPF.TileCanvas package to NuGet.org.

## Prerequisites

1. **NuGet.org Account**: Create account at https://www.nuget.org/
2. **API Key**: Generate API key at https://www.nuget.org/account/apikeys
3. **Package Name**: Reserve `CodingConnected.WPF.TileCanvas` (if available)

## Quick Start

### 1. Dry Run (Test Everything)
```powershell
# Test the entire workflow without publishing
.\publish-nuget.ps1 -DryRun
```

### 2. First Publish
```powershell
# Publish version 1.0.0 with your API key
.\publish-nuget.ps1 -ApiKey "your-api-key-here"
```

### 3. Update Version and Publish
```powershell
# Publish new version 1.1.0
.\publish-nuget.ps1 -Version "1.1.0" -ApiKey "your-api-key-here"
```

## Publishing Workflow Steps

The script automatically handles:

1. **📋 Validation**
   - Checks project file exists
   - Validates version numbers
   - Shows current/target versions

2. **🔨 Clean Build**
   - Cleans previous builds
   - Builds in Release configuration
   - Creates NuGet package

3. **📦 Package Location**
   - Finds generated .nupkg file
   - Shows package size and details

4. **✍️ Package Signing** (Optional)
   - Signs with your CodingConnected certificate
   - Uses existing sign-package.ps1 script
   - Can be skipped with -SkipSigning

5. **🔍 Version Check**
   - Checks if version already exists
   - Prevents accidental overwrites

6. **🚀 Publishing**
   - Uploads to NuGet.org
   - Provides success confirmation
   - Shows package URL and install commands

## Command Options

| Parameter | Description | Example |
|-----------|-------------|---------|
| `-Version` | Specific version to publish | `-Version "1.2.0"` |
| `-ApiKey` | NuGet.org API key | `-ApiKey "oy2b...xyz"` |
| `-DryRun` | Test without publishing | `-DryRun` |
| `-SkipSigning` | Skip package signing | `-SkipSigning` |
| `-Force` | Overwrite existing version | `-Force` (not recommended) |
| `-Source` | Custom NuGet source | `-Source "https://custom.nuget/"` |

## Version Management

### Semantic Versioning
Follow semantic versioning (semver): `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes (1.0.0 → 2.0.0)
- **MINOR**: New features, backwards compatible (1.0.0 → 1.1.0)  
- **PATCH**: Bug fixes, backwards compatible (1.0.0 → 1.0.1)

### Pre-release Versions
For alpha/beta releases:
```powershell
.\publish-nuget.ps1 -Version "1.1.0-alpha" -ApiKey "your-key"
.\publish-nuget.ps1 -Version "1.1.0-beta.1" -ApiKey "your-key"
```

## API Key Management

### Option 1: Per-Command (Recommended)
```powershell
.\publish-nuget.ps1 -ApiKey "your-key-here"
```

### Option 2: Global Configuration
```powershell
# Set once, use everywhere (less secure)
nuget setApiKey "your-key-here"
.\publish-nuget.ps1
```

### Option 3: Environment Variable
```powershell
$env:NUGET_API_KEY = "your-key-here"
.\publish-nuget.ps1 -ApiKey $env:NUGET_API_KEY
```

## Security Best Practices

1. **Never commit API keys** to source control
2. **Use scoped API keys** with minimal permissions
3. **Rotate keys regularly**
4. **Use environment variables** for CI/CD

## Troubleshooting

### "Package already exists"
```
Error: Version 1.0.0 already exists on NuGet.org
Solution: Use a new version number or -Force (not recommended)
```

### "Authentication failed"
```
Error: 401 Unauthorized
Solution: Check API key is correct and has push permissions
```

### "Signing failed"
```
Warning: Package signing failed
Solution: Ensure hardware token connected, or use -SkipSigning
```

### "Package too large"
```
Error: Package exceeds size limit
Solution: Review package contents, exclude unnecessary files
```

## Post-Publishing Checklist

After successful publishing:

1. **✅ Verify on NuGet.org**: Visit package URL
2. **✅ Test Installation**: Try `dotnet add package CodingConnected.WPF.TileCanvas`
3. **✅ Update Documentation**: Update README with new version
4. **✅ Tag Release**: Create git tag for version
5. **✅ Release Notes**: Document changes on GitHub

## CI/CD Integration

For automated publishing:

```yaml
# GitHub Actions example
- name: Publish to NuGet
  run: |
    .\publish-nuget.ps1 -ApiKey ${{ secrets.NUGET_API_KEY }} -Version ${{ github.ref_name }}
```

## Package Statistics

After publishing, monitor:
- **Downloads**: Package popularity
- **Dependencies**: Who's using your package
- **Issues**: Bug reports and feature requests
- **Versions**: Usage distribution

## Support

- **NuGet.org Issues**: https://github.com/NuGet/NuGetGallery/issues
- **Package Management**: https://docs.microsoft.com/en-us/nuget/
- **Semantic Versioning**: https://semver.org/