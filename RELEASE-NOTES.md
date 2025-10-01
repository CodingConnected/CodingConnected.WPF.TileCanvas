# Release Notes

## Version 1.0.1 (2025-10-01)

### ✨ New Features
- **Optional Headers**: Panes can now be created without headers, allowing for cleaner layouts when header functionality is not needed

### 🔧 Technical Details
- Added support for headerless pane configuration
- Maintains full backward compatibility with existing implementations

---

## Version 1.0.0 (2024-09-30)

🎉 **Initial Release - CodingConnected.WPF.TileCanvas**

### Overview
First public release of the CodingConnected WPF TileCanvas library - a flexible, responsive tile canvas system for WPF applications with drag-and-drop, resizing, and grid-based layout management.

### ✨ Features

#### Core Functionality
- **Interactive Drag & Drop**: Panels can be dragged around the canvas with real-time visual feedback
- **Resizable Panels**: Panels support mouse-driven resizing with optional grid snapping
- **Dual Grid System**: 
  - **Fixed Mode**: Traditional grid with fixed cell sizes (20px default)
  - **Flexible Mode**: Column-based responsive layout that adapts to container width

#### MVVM Integration  
- **Full Data Binding**: Supports `ItemsSource` binding with `IPaneViewModel` interface
- **Template Support**: Use `DataTemplateSelector` for custom panel content
- **Two-Way Synchronization**: Changes in UI automatically update ViewModels and vice versa
- **Property Change Notifications**: Real-time updates when ViewModels change

#### Layout Management
- **Persistence**: Save/load layouts to JSON files with `ILayoutSerializer`
- **Grid Snapping**: Optional snapping during drag and resize operations
- **Edit Modes**: Toggle between edit and view modes
- **Event System**: Comprehensive events for panel lifecycle and layout changes

#### Professional Features
- **Signed Package**: Code-signed with CodingConnected certificate for trust and integrity
- **Full Documentation**: Complete XML documentation for IntelliSense support
- **Custom Styling**: Configurable panel colors, spacing, margins, and visual appearance
- **Performance**: Optimized for smooth interactions with large numbers of panels

### 🛠️ Technical Specifications
- **Target Framework**: .NET 8.0 Windows
- **Dependencies**: CommunityToolkit.Mvvm 8.4.0
- **Package Size**: ~44KB
- **License**: MIT

### 📦 Installation
```
Install-Package CodingConnected.WPF.TileCanvas
```
or
```
dotnet add package CodingConnected.WPF.TileCanvas
```

### 🚀 Quick Start
```xml
<tiles:TileCanvas ItemsSource="{Binding Panes}"
                  IsEditMode="True"
                  GridMode="Flexible"
                  ShowGrid="True" />
```

### 🔧 Key Classes and Interfaces
- `TileCanvas` - Main canvas control
- `TilePanel` - Individual panel control  
- `IPaneViewModel` - Interface for MVVM binding
- `ILayoutSerializer` - Interface for save/load functionality
- `CanvasConfiguration` - Configuration settings
- `GridConfiguration` - Grid-specific settings

### 📚 Documentation
- **README.md**: Comprehensive usage guide with examples
- **SIGNING.md**: Package signing documentation  
- **PUBLISHING.md**: Publishing workflow guide
- **XML Documentation**: Full API documentation for IntelliSense

### 🎯 Use Cases
- Dashboard applications
- Layout designers
- Widget containers
- Tile-based interfaces
- Drag-and-drop interfaces
- Responsive panel layouts

### 🔗 Links
- **NuGet Package**: https://www.nuget.org/packages/CodingConnected.WPF.TileCanvas/
- **Source Code**: https://github.com/CodingConnected/CodingConnected.WPF.TileCanvas
- **Documentation**: See README.md in repository
- **Issues**: GitHub Issues for bug reports and feature requests

### 👨‍💻 Author
**CodingConnected e.U.**  
Professional WPF control development and consulting

---

*This is the initial stable release. Future updates will maintain backwards compatibility following semantic versioning.*