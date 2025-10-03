# CodingConnected.WPF.TileCanvas

[![NuGet version (CodingConnected.WPF.TileCanvas)](https://img.shields.io/nuget/v/CodingConnected.WPF.TileCanvas.svg?style=flat-square)](https://www.nuget.org/packages/CodingConnected.WPF.TileCanvas/)

A flexible, responsive tile canvas system for WPF applications with drag-and-drop, resizing, and grid-based layout management.

## Features

- **Drag & Drop**: Interactive dragging of tiles with real-time visual feedback
- **Resizable Panels**: Panels can be resized with mouse interaction
- **Grid System**: Two grid modes - Fixed and Flexible responsive layouts
- **MVVM Support**: Full data binding support with `IPaneViewModel` interface
- **Layout Persistence**: Save and load layouts with library-level or simplified ViewModel-only serialization
- **Edit Mode**: Toggle between edit and view modes
- **Grid Snapping**: Optional snapping to grid during drag/resize operations
- **Panel Selection**: Click to select panels with `SelectedPaneId` property and `IsSelected` ViewModel support
- **Customizable**: Configurable grid spacing, panel margins, and visual styling
- **Template Support**: Automatic WPF template resolution or optional `DataTemplateSelector` for advanced scenarios

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
    public bool IsSelected { get; set; }

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

## Panel Selection

TileCanvas supports panel selection through click interaction:

```xml
<!-- Bind to SelectedPaneId property -->
<tiles:TileCanvas ItemsSource="{Binding Panes}"
                  SelectedPaneId="{Binding SelectedPaneId}" />
```

```csharp
// React to selection changes in your ViewModel
public string SelectedPaneId
{
    get => _selectedPaneId;
    set => SetProperty(ref _selectedPaneId, value);
}

// Or handle the PanelSelected event
tileCanvas.PanelSelected += (sender, e) => {
    Console.WriteLine($"Panel {e.Panel.Id} selected");
};
```

The `IsSelected` property on `IPaneViewModel` is automatically synchronized with the canvas selection.

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

**Library-Level Serialization** (Basic panel layout data):
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

**Application-Level Serialization** (Complete app state via ViewModels):
```csharp
// For applications using MVVM with IPaneViewModel collections
// This approach saves complete application state in a single file

var serializer = new ViewModelSerializer();
var appSettings = new AppSettings { /* your app configuration */ };

// Save complete app state (ViewModels + settings)
await serializer.SaveAsync(viewModels, appSettings, "myapp-layout.json");

// Load complete app state
var (loadedViewModels, loadedSettings) = await serializer.LoadAsync("myapp-layout.json");
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

## Serialization Architecture

### When to Use Library-Level Serialization

- **Simple applications** that only need basic panel layout persistence
- **Direct TilePanel usage** without MVVM ViewModels
- **Quick prototyping** or minimal applications
- When you don't need to persist application-specific data

### When to Use Application-Level (ViewModel) Serialization

- **MVVM applications** using `IPaneViewModel` collections  
- **Complex applications** with type-specific data (chart settings, statistics, etc.)
- **Complete application state** needs to be persisted
- **Single-file simplicity** is preferred over dual-file complexity
- **Clean separation of concerns** between UI library and application logic

### Recommended Pattern for MVVM Applications

```csharp
// 1. Create your ViewModels implementing IPaneViewModel
public class ChartPaneViewModel : PaneViewModel
{
    [ObservableProperty] private string _chartType = "Line";
    [ObservableProperty] private bool _showLegend = true;
    // ... other chart-specific properties
}

// 2. Use ViewModelSerializer for complete app state
public class MainViewModel
{
    public ObservableCollection<IPaneViewModel> Panes { get; }
    
    [RelayCommand]
    private async Task SaveLayoutAsync()
    {
        var serializer = new ViewModelSerializer();
        var appSettings = CreateAppSettings();
        await serializer.SaveAsync(Panes, appSettings, fileName);
    }
}

// 3. Let TileCanvas auto-sync via MVVM binding
// <TileCanvas ItemsSource="{Binding Panes}" ... />
```

This approach provides **maximum flexibility** with **minimal complexity**.

### Migration from Dual-File Serialization

If you're currently using a complex dual-file approach (library layout + app data), consider migrating to ViewModel-only serialization:

**Benefits:**
- ✅ **Single file** instead of `.json` + `.appdata.json` pairs
- ✅ **~400 lines less code** in your application
- ✅ **No library coupling** - you control your own serialization
- ✅ **Better maintainability** with separated concerns
- ✅ **Same functionality** with much simpler implementation

See the example project for a complete implementation that went from 162 lines of complex save/load logic down to just 45 lines of simple code!

## Events

The TileCanvas raises several events for monitoring layout changes:

```csharp
tileCanvas.PanelAdded += (sender, e) => { /* Panel added */ };
tileCanvas.PanelRemoved += (sender, e) => { /* Panel removed */ };
tileCanvas.PanelMoved += (sender, e) => { /* Panel moved */ };
tileCanvas.PanelResized += (sender, e) => { /* Panel resized */ };
tileCanvas.PanelSelected += (sender, e) => { /* Panel selected */ };
tileCanvas.LayoutChanged += (sender, e) => { /* Layout changed */ };
```

## Template Resolution

TileCanvas supports two approaches for resolving DataTemplates for your ViewModels:

### 1. Automatic Template Resolution (Recommended)

The simplest approach uses WPF's built-in template matching. Just define DataTemplates with `DataType` attributes:

```xml
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/PaneDataTemplates.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>

<!-- In PaneDataTemplates.xaml -->
<DataTemplate DataType="{x:Type viewModels:ChartPaneViewModel}">
    <local:ChartUserControl />
</DataTemplate>

<DataTemplate DataType="{x:Type viewModels:StatsPaneViewModel}">
    <local:StatsUserControl />
</DataTemplate>

<!-- TileCanvas usage - no template selector needed -->
<tiles:TileCanvas ItemsSource="{Binding Panes}" />
```

**Benefits:**
- ✅ Simple and clean - no extra selector classes needed
- ✅ WPF automatically finds the right template
- ✅ Standard WPF pattern
- ✅ Easy to maintain and extend

### 2. Explicit Template Selector (Advanced)

For complex scenarios requiring custom logic, use `DataTemplateSelector`:

```xml
<!-- Define keyed templates -->
<DataTemplate x:Key="ChartTemplate" DataType="{x:Type viewModels:ChartPaneViewModel}">
    <local:ChartUserControl />
</DataTemplate>

<DataTemplate x:Key="StatsTemplate" DataType="{x:Type viewModels:StatsPaneViewModel}">
    <local:StatsUserControl />
</DataTemplate>

<!-- Create template selector -->
<local:MyTemplateSelector x:Key="MyTemplateSelector"
                         ChartTemplate="{StaticResource ChartTemplate}"
                         StatsTemplate="{StaticResource StatsTemplate}" />

<!-- Use explicit selector -->
<tiles:TileCanvas ItemsSource="{Binding Panes}"
                  PaneContentTemplateSelector="{StaticResource MyTemplateSelector}" />
```

**When to use:**
- ✅ Complex template selection logic beyond type matching
- ✅ Runtime template switching based on ViewModel properties
- ✅ Backward compatibility with existing selector implementations

**Note:** The example project demonstrates the automatic approach for simplicity.

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
- **MVVM pattern implementation** with `IPaneViewModel` interface
- **Simplified ViewModel-only serialization** - single file saves complete app state
- **Custom DataTemplateSelector usage** for different panel types
- **Different panel types** (Charts, Stats, Tables, Labels) with type-specific properties
- **Grid mode switching** between Fixed and Flexible layouts
- **Interactive editing features** (drag, resize, close panels)
- **Clean architecture** with separated serialization concerns

### Key Example Features:

**Simple Save/Load Implementation:**
```csharp
// MainViewModel save method - just 20 lines!
[RelayCommand]
private async Task SaveLayoutAsync()
{
    var serializer = new ViewModelSerializer();
    var appSettings = new AppSettings { /* capture all app state */ };
    await serializer.SaveAsync(Panes, appSettings, fileName);
}

// MainViewModel load method - just 25 lines!
[RelayCommand]
private async Task LoadLayoutAsync()
{
    var serializer = new ViewModelSerializer();
    var (panes, appSettings) = await serializer.LoadAsync(fileName);
    // Apply settings and replace panes - done!
}
```

The example demonstrates how **ViewModel serialization alone is sufficient** for complete application state persistence, eliminating the need for complex dual-file approaches.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or feature requests, please open an issue on the GitHub repository.