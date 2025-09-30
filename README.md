# CodingConnected.WPF.TileCanvas

[![NuGet version (CodingConnected.WPF.TileCanvas)](https://img.shields.io/nuget/v/CodingConnected.WPF.TileCanvas.svg?style=flat-square)](https://www.nuget.org/packages/CodingConnected.WPF.TileCanvas/)

A flexible, responsive tile canvas system for WPF applications with drag-and-drop, resizing, and grid-based layout management.

## Features

- **Drag & Drop**: Interactive dragging of tiles with real-time visual feedback
- **Resizable Panels**: Panels can be resized with mouse interaction
- **Grid System**: Two grid modes - Fixed and Flexible responsive layouts
- **MVVM Support**: Full data binding support with `IPaneViewModel` interface
- **Layout Persistence**: Save and load layouts to/from JSON files
- **Edit Mode**: Toggle between edit and view modes
- **Grid Snapping**: Optional snapping to grid during drag/resize operations
- **Customizable**: Configurable grid spacing, panel margins, and visual styling
- **Template Support**: Use `DataTemplateSelector` for custom panel content

## Installation

Install via NuGet Package Manager:

```
Install-Package CodingConnected.WPF.TileCanvas
```

Or via .NET CLI:

```
dotnet add package CodingConnected.WPF.TileCanvas
```

## Quick Start

### Basic XAML Usage

```xml
<Window xmlns:tiles="clr-namespace:CodingConnected.WPF.TileCanvas.Library.Controls;assembly=CodingConnected.WPF.TileCanvas.Library">
    <tiles:TileCanvas Name="MyTileCanvas"
                      IsEditMode="True"
                      ShowGrid="True"
                      GridMode="Flexible"
                      ColumnCount="7"
                      MinColumnWidth="100" />
</Window>
```

### MVVM Data Binding

```xml
<tiles:TileCanvas ItemsSource="{Binding Panes}"
                  PaneContentTemplateSelector="{StaticResource MyTemplateSelector}"
                  IsEditMode="{Binding IsEditMode}"
                  GridMode="{Binding GridMode}" />
```

### ViewModel Implementation

```csharp
public class MyPaneViewModel : IPaneViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string PaneType { get; set; }
    public string HeaderColor { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

## Grid Modes

### Fixed Grid Mode
- Fixed grid size (default: 20px)
- Panels can be positioned anywhere on a uniform grid
- Canvas size grows dynamically based on panel positions

### Flexible Grid Mode
- Column-based responsive layout
- Configurable column count and minimum widths
- Columns resize based on available space
- Ideal for dashboard-style layouts

## Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsEditMode` | `bool` | Enable/disable panel editing (drag, resize, close) |
| `GridMode` | `GridMode` | Fixed or Flexible grid layout mode |
| `ShowGrid` | `bool` | Show/hide grid lines |
| `ColumnCount` | `int` | Number of columns (Flexible mode) |
| `MinColumnWidth` | `int` | Minimum column width in pixels |
| `SnapToGridOnDrag` | `bool` | Snap panels to grid during dragging |
| `SnapToGridOnResize` | `bool` | Snap panels to grid during resizing |
| `PanelSpacing` | `double` | Visual spacing between panel content |
| `PanelGap` | `double` | Gap between panels |

## Methods

### Panel Management
```csharp
// Add a panel programmatically
tileCanvas.AddPanel(new TilePanel { Title = "My Panel" }, x: 100, y: 50);

// Remove a panel
tileCanvas.RemovePanel(panel);

// Clear all panels
tileCanvas.ClearPanels();
```

### Layout Persistence
```csharp
// Save layout to file
await tileCanvas.SaveLayoutAsync("layout.json");

// Load layout from file
await tileCanvas.LoadLayoutAsync("layout.json");

// Serialize to string
string layoutData = tileCanvas.SerializeLayout();

// Deserialize from string
tileCanvas.DeserializeLayout(layoutData);
```

### Grid Operations
```csharp
// Snap all panels to grid
tileCanvas.SnapAllPanelsToGrid();

// Snap a position to grid
Point snappedPosition = tileCanvas.SnapToGrid(position);

// Snap a size to grid
Size snappedSize = tileCanvas.SnapSizeToGrid(size);
```

## Events

The TileCanvas raises several events for monitoring layout changes:

```csharp
tileCanvas.PanelAdded += (sender, e) => { /* Panel added */ };
tileCanvas.PanelRemoved += (sender, e) => { /* Panel removed */ };
tileCanvas.PanelMoved += (sender, e) => { /* Panel moved */ };
tileCanvas.PanelResized += (sender, e) => { /* Panel resized */ };
tileCanvas.LayoutChanged += (sender, e) => { /* Layout changed */ };
```

## Custom Panel Content

Use `DataTemplateSelector` to provide different templates based on panel type:

```xml
<DataTemplate x:Key="ChartTemplate">
    <local:ChartUserControl />
</DataTemplate>

<DataTemplate x:Key="StatsTemplate">
    <local:StatsUserControl />
</DataTemplate>

<local:MyTemplateSelector x:Key="MyTemplateSelector"
                         ChartTemplate="{StaticResource ChartTemplate}"
                         StatsTemplate="{StaticResource StatsTemplate}" />
```

## Configuration

Customize the canvas appearance and behavior:

```csharp
var config = new CanvasConfiguration
{
    IsEditMode = true,
    Grid = new GridConfiguration
    {
        Mode = GridMode.Flexible,
        ColumnCount = 12,
        MinColumnWidth = 100,
        ShowGrid = true,
        GridSize = 20,
        GridLineColor = "#E0E0E0"
    }
};

tileCanvas.Configuration = config;
```

## Requirements

- .NET 8.0 or later
- Windows (WPF)
- CommunityToolkit.Mvvm 8.4.0+

## Examples

Check out the included example project (`CodingConnected.WPF.TileCanvas.Example`) for a complete demonstration of:
- MVVM pattern implementation
- Custom DataTemplateSelector usage
- Layout persistence
- Different panel types (Charts, Stats, Tables)
- Grid mode switching
- Interactive editing features

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or feature requests, please open an issue on the GitHub repository.