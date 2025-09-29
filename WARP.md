# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is a WPF (.NET 8) application that implements a draggable tile/panel canvas system, similar to dashboard builders like Grafana or modern BI tools. The application provides a grid-based layout system with drag-and-drop functionality for repositioning panels.

## Build Commands

### Core Development Commands
```powershell
# Build the project
dotnet build CodingConnected.WPF.TileCanvas.csproj

# Build in Release mode
dotnet build CodingConnected.WPF.TileCanvas.csproj --configuration Release

# Run the application
dotnet run --project CodingConnected.WPF.TileCanvas.csproj

# Clean build artifacts
dotnet clean CodingConnected.WPF.TileCanvas.csproj
```

### Platform-Specific Builds
```powershell
# Build for specific platform
dotnet build --configuration Release --runtime win-x64
dotnet build --configuration Release --runtime win-x86

# Create self-contained executable
dotnet publish --configuration Release --runtime win-x64 --self-contained true
```

### Solution-Level Commands
```powershell
# Build entire solution
dotnet build CodingConnected.WPF.TileCanvas.sln

# Restore NuGet packages
dotnet restore
```

## Architecture Overview

### Core Components

#### MainWindow (MainWindow.xaml/.cs)
- **Primary UI Container**: Houses the toolbar, dashboard canvas, and grid overlay
- **Event Orchestration**: Manages all user interactions, drag operations, and grid calculations
- **State Management**: Handles edit mode, grid modes, and layout persistence

#### Grid System Architecture
The application implements two distinct grid systems:

**Fixed Grid Mode** (`_isFlexibleGrid = false`):
- Uses uniform grid cells of configurable size (25px, 50px, 100px)
- Panels snap to regular grid intersections
- Simpler positioning logic with `Math.Round(position / _gridSize) * _gridSize`

**Flexible Grid Mode** (`_isFlexibleGrid = true`):
- Column-based responsive layout system
- Configurable column count and minimum column width
- Panels snap to column boundaries but maintain fixed vertical grid
- Complex width calculation that distributes available space across columns

#### Panel Management System
Each dashboard panel is implemented as a `Border` containing:
- **Header**: Draggable title bar with close button (edit mode only)
- **Content Area**: Placeholder content with resize instructions
- **Resize Handle**: Bottom-right resize thumb (edit mode only)

#### Layout Persistence
- JSON-based serialization via `PanelLayout` class
- Saves to `dashboard-layout.json` in application directory
- Preserves position, size, title, and header color for each panel

### Key Architectural Patterns

#### Drag-and-Drop Implementation
- **Initiation**: Header `MouseDown` captures start position and element reference
- **Movement**: Canvas-level `MouseMove` calculates delta and applies appropriate grid snapping
- **Completion**: Canvas-level `MouseUp` cleans up drag state and resets Z-index

#### Grid Snapping Logic
The snapping system adapts based on current grid mode:
- Fixed mode: Simple modulo arithmetic
- Flexible mode: Complex column boundary detection with `SnapToFlexibleGrid()`

#### Edit Mode Toggle
- Controls visibility of resize handles and close buttons
- Manages cursor states and event handler attachment/detachment
- Ensures clean separation between edit and view modes

### Data Flow

1. **User Interaction** → Event handlers in MainWindow
2. **Grid Calculation** → `CalculateColumnWidths()` or fixed grid math  
3. **Position/Size Update** → Canvas positioning and element sizing
4. **Visual Feedback** → Grid line rendering and panel updates
5. **Persistence** → JSON serialization on save operations

## Development Guidance

### Grid System Modifications
When modifying grid behavior:
- Both `SnapToFlexibleGrid()` and fixed grid logic must be updated consistently
- Grid line rendering in `DrawFlexibleGridLines()` and `DrawFixedGridLines()` should match positioning logic
- Column width calculations affect both snapping and visual grid display

### Panel Behavior Extensions
To add new panel features:
- Modify `CreateDashboardPanel()` method to add UI elements
- Update `PanelLayout` class to include new properties for persistence
- Ensure edit mode logic in `UpdatePanelEditMode()` handles new elements

### Layout System Changes  
For layout modifications:
- Update JSON serialization in `SaveLayoutToFile()` and `LoadLayoutFromFile()`
- Consider backward compatibility with existing saved layouts
- Test with both grid modes to ensure consistent behavior

### Drag-and-Drop Enhancements
When extending drag functionality:
- Mouse event handling occurs at multiple levels (header → canvas)
- Z-index management prevents panels from getting stuck behind others
- Grid snapping must account for canvas boundaries to prevent panels from disappearing

### Performance Considerations
- Grid line rendering redraws on every resize/mode change
- Panel position updates during drag operations should be efficient
- Large numbers of panels may require virtualization or optimization