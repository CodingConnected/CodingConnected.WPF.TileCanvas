# Publishing Quick Reference

## 🚀 Ready to Publish Commands

### Test Everything First
```powershell
# Dry run - validates entire workflow without publishing
.\publish-nuget.ps1 -DryRun
```

### Publish Options

#### Option 1: With Signing (Hardware Token)
```powershell
# Will prompt for certificate PIN/password and API key
.\publish-nuget.ps1
```

#### Option 2: Skip Signing (Faster for testing)
```powershell
# Skip signing step - faster for initial testing
.\publish-nuget.ps1 -SkipSigning
```

#### Option 3: New Version
```powershell
# Publish new version (automatically updates project file)
.\publish-nuget.ps1 -Version "1.0.1"
```

## 📋 Pre-Publishing Checklist

- [ ] Package name `CodingConnected.WPF.TileCanvas` reserved on NuGet.org
- [ ] API key obtained from https://www.nuget.org/account/apikeys
- [ ] Dry run completed successfully: `.\publish-nuget.ps1 -DryRun`
- [ ] Hardware token connected (if using signing)
- [ ] Version number ready (current: 1.0.0)

## 🎯 Next Steps After Publishing

1. Visit: https://www.nuget.org/packages/CodingConnected.WPF.TileCanvas/
2. Test: `dotnet add package CodingConnected.WPF.TileCanvas`  
3. Update: Repository README with NuGet badge
4. Tag: Git release with version number
5. Monitor: Package download statistics

## 🆘 Need Help?

- **API Key Issues**: Check https://www.nuget.org/account/apikeys
- **Signing Problems**: Use `-SkipSigning` or switch to Store method
- **Version Conflicts**: Increment version number or check existing packages
- **Build Errors**: Run `dotnet clean` and try again

---
**Package ID**: `CodingConnected.WPF.TileCanvas`  
**Current Version**: `1.0.0`  
**Package Size**: ~44KB  
**Target Framework**: `.NET 8.0 Windows`