# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is a **refactored WPF (.NET 8) tile canvas library** consisting of two main projects:
- **Library**: `CodingConnected.WPF.TileCanvas.Library` - A reusable WPF library for creating draggable dashboard layouts
- **Example**: `CodingConnected.WPF.TileCanvas.Example` - A demonstration application showing library usage

The library implements a sophisticated grid-based layout system with drag-and-drop functionality, similar to modern dashboard builders like Grafana or Power BI.

## Project Structure

```
src/
├── CodingConnected.WPF.TileCanvas.Library/     # Main library project
│   ├── Controls/                              # UserControls (TileCanvas, TilePanel)
│   ├── Enums/                                 # EditMode, GridMode
│   ├── Events/                                # Event argument classes
│   ├── Models/                                # Configuration and layout models
│   └── Services/                              # Grid calculation and serialization
└── CodingConnected.WPF.TileCanvas.Example/    # Example application
```

## Build Commands

### Solution-Level Commands (Recommended)
```powershell
# Build entire solution
dotnet build CodingConnected.WPF.TileCanvas.sln

# Build in Release mode
dotnet build CodingConnected.WPF.TileCanvas.sln --configuration Release

# Run the example application
dotnet run --project src/CodingConnected.WPF.TileCanvas.Example/CodingConnected.WPF.TileCanvas.Example.csproj

# Clean solution
dotnet clean CodingConnected.WPF.TileCanvas.sln

# Restore NuGet packages
dotnet restore CodingConnected.WPF.TileCanvas.sln
```

### Library-Specific Commands
```powershell
# Build library only
dotnet build src/CodingConnected.WPF.TileCanvas.Library/CodingConnected.WPF.TileCanvas.Library.csproj

# Create NuGet package
dotnet pack src/CodingConnected.WPF.TileCanvas.Library/CodingConnected.WPF.TileCanvas.Library.csproj

# Publish library
dotnet publish src/CodingConnected.WPF.TileCanvas.Library/CodingConnected.WPF.TileCanvas.Library.csproj
```

### Platform-Specific Builds
```powershell
# Build for specific platforms
dotnet build --configuration Release --runtime win-x64
dotnet build --configuration Release --runtime win-x86

# Create self-contained executable (Example app)
dotnet publish src/CodingConnected.WPF.TileCanvas.Example/CodingConnected.WPF.TileCanvas.Example.csproj --configuration Release --runtime win-x64 --self-contained true
```

## Architecture Overview

### Library Architecture (Separation of Concerns)

The refactored architecture follows clean separation principles:

#### Core Controls (`Controls/`)
**TileCanvas**: Main container control
- Manages panel collections via `ObservableCollection<TilePanel>`
- Handles grid rendering, drag operations, and layout events
- Provides public API for adding/removing panels, serialization
- Uses dependency injection for layout serialization

**TilePanel**: Individual panel control
- Self-contained UserControl with header, content, and resize functionality
- Raises events for drag start, resize, and close operations
- Supports templating for custom appearance

#### Configuration Models (`Models/`)
**CanvasConfiguration**: Central configuration object
- `EditMode`: View/Edit mode enumeration
- `GridConfiguration`: Grid-specific settings
- Animation and styling properties

**GridConfiguration**: Grid system settings
- `GridMode`: Fixed vs Flexible grid modes
- Column count, sizing, snap behavior
- Visual properties (colors, thickness)

**PanelLayout**: Serializable panel data
- Position, size, styling information
- Used for save/load operations

#### Services (`Services/`)
**GridCalculationService**: Grid mathematics
- Column width calculations for flexible mode
- Position and size snapping algorithms
- Grid boundary calculations

**ILayoutSerializer / JsonLayoutSerializer**: Persistence
- Abstract interface for different serialization formats
- JSON implementation for save/load operations
- Async file operations

### Grid System Architecture

**Fixed Grid Mode (`GridMode.Fixed`)**:
- Uniform grid cells of configurable size
- Simple position snapping: `Math.Round(pos / gridSize) * gridSize`
- Unlimited canvas expansion

**Flexible Grid Mode (`GridMode.Flexible`)**:
- Responsive column-based layout system
- Column widths calculated based on available space
- Horizontal constraints, vertical expansion
- Complex snapping to column boundaries

### Event System

The library implements a comprehensive event system:
- `PanelAdded`, `PanelRemoved`, `PanelMoved`, `PanelResized`
- `LayoutChanged` with change type enumeration
- Clean event subscription/unsubscription patterns

### Key Design Patterns

#### Dependency Injection
- `ILayoutSerializer` interface allows custom serialization implementations
- Service classes are injected rather than tightly coupled

#### Observable Collections
- `ObservableCollection<TilePanel>` for automatic UI updates
- Event handlers for collection changes

#### Template-Based Controls
- Both TileCanvas and TilePanel use ControlTemplates
- Named template parts (`PART_*`) for extensibility

#### Configuration-Driven Behavior
- Single `CanvasConfiguration` object controls all behavior
- Changes to configuration automatically update UI state

## Development Guidance

### Extending the Library

#### Adding New Grid Modes
1. Add enum value to `GridMode`
2. Update `GridCalculationService` with new calculation logic
3. Add grid line rendering in `TileCanvas.DrawGridLines()`
4. Update example UI to demonstrate new mode

#### Custom Panel Types
1. Derive from `TilePanel` or create new UserControl
2. Implement `GetLayout()` and `FromLayout()` methods
3. Update `PanelLayout` model if new properties needed
4. Consider custom serialization requirements

#### Alternative Serialization Formats
1. Implement `ILayoutSerializer` interface
2. Handle conversion to/from `PanelLayout[]`
3. Implement async file operations
4. Set custom serializer on `TileCanvas.LayoutSerializer`

### Library Usage Patterns

#### Basic Usage (see Example project)
```xml
<tiles:TileCanvas x:Name="TileCanvas"
                 PanelAdded="TileCanvas_PanelAdded"
                 LayoutChanged="TileCanvas_LayoutChanged"/>
```

```csharp
// Add panel
var panel = new TilePanel { Title = "My Panel", Width = 300, Height = 200 };
TileCanvas.AddPanel(panel, 0, 0);

// Configure grid
TileCanvas.GridMode = GridMode.Flexible;
TileCanvas.Configuration.Grid.ColumnCount = 12;

// Save/Load
await TileCanvas.SaveLayoutAsync("layout.json");
await TileCanvas.LoadLayoutAsync("layout.json");
```

### Testing and Debugging

#### Common Issues
- **Nullable reference warnings**: Enable nullable context in library projects
- **Template part access**: Ensure `OnApplyTemplate()` is called before accessing PART_* elements  
- **Grid calculations**: Test both Fixed and Flexible modes when modifying grid logic
- **Event memory leaks**: Always clean up event subscriptions in `CleanupPanelEvents()`

#### Testing Strategy
1. **Unit Tests**: Test `GridCalculationService` and serialization separately
2. **Integration Tests**: Test full drag/drop/resize workflows
3. **Visual Tests**: Verify grid rendering in both modes
4. **Performance Tests**: Test with large numbers of panels

### Packaging and Distribution

The library is configured for NuGet packaging:
- `GeneratePackageOnBuild=true` in Library project
- Package metadata configured in `.csproj`
- Automatic versioning and dependency management
