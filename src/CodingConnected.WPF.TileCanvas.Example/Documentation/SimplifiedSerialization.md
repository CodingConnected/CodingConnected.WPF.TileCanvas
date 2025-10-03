# Simplified ViewModel-Only Serialization

## Overview

The simplified serialization approach eliminates the dual-file system and uses only ViewModels to save/load pane data. This provides:

- **Single File**: All app state in one `.viewmodels.json` file
- **No Library Dependency**: No need for TileCanvas library's `ILayoutSerializer`
- **Complete Data**: All pane positioning, styling, and app-specific data preserved
- **Cleaner Code**: Simpler save/load logic with less complexity

## Usage

### Saving Layouts
```csharp
// Instead of the complex dual-file approach:
await mainViewModel.SaveLayoutAsync();  // OLD: Creates .json + .appdata.json

// Use the simplified approach:
await mainViewModel.SaveLayoutSimplifiedAsync();  // NEW: Creates single .viewmodels.json
```

### Loading Layouts
```csharp
// Instead of the complex dual-file loading:
await mainViewModel.LoadLayoutAsync();  // OLD: Loads .json + .appdata.json

// Use the simplified approach:  
await mainViewModel.LoadLayoutSimplifiedAsync();  // NEW: Loads single .viewmodels.json
```

## Data Structure

The saved file contains everything needed to restore the application:

```json
{
  "appSettings": {
    "panelCounter": 3,
    "gridMode": "Flexible",
    "showGrid": true,
    "snapDrag": true,
    "snapResize": true,
    "columnCount": 7,
    "minColumnWidth": 100,
    "isEditMode": true,
    "panelSpacing": 5,
    "panelGap": 2
  },
  "panes": [
    {
      "id": "guid-here",
      "title": "Chart Panel",
      "x": 0,
      "y": 0,
      "width": 400,
      "height": 300,
      "headerColor": "#FF87CEEB",
      "backgroundColor": "#FFFFFFFF",
      "borderColor": "#FFBBBBBB",
      "borderThickness": 2,
      "showHeader": true,
      "canClose": true,
      "isSelected": false,
      "paneType": "Chart",
      "typeSpecificData": {
        "chartType": "Line",
        "dataDescription": "Sample chart data",
        "seriesInfo": "📊 Chart Panel...",
        "showLegend": true
      }
    }
  ],
  "savedAt": "2024-10-03T07:45:00Z",
  "appVersion": "1.0.0.0"
}
```

## Benefits

1. **Eliminates Redundancy**: No duplicate data between layout file and app data file
2. **Simplified Logic**: Single serialization path instead of dual conversion process
3. **Better Maintainability**: All serialization logic in one place
4. **No Library Coupling**: App controls its own save/load without library interface constraints
5. **Complete State**: ViewModels contain all necessary information for restoration

## How It Works

1. **Save Process**: 
   - Extract all ViewModel data (position, size, colors, type-specific properties)
   - Capture app settings (grid mode, counters, etc.)
   - Serialize everything to single JSON file

2. **Load Process**:
   - Deserialize JSON file into ViewModels and app settings
   - Restore ViewModels with all properties
   - Apply app settings to MainViewModel
   - Let TileCanvas auto-sync from ViewModels (existing MVVM binding)

## Migration

To migrate from dual-file to single-file:

1. **Add** the `ViewModelSerializer` class
2. **Add** the extension methods with simplified save/load
3. **Add** new commands to MainViewModel
4. **Test** that existing functionality is preserved
5. **Optionally** keep existing methods for backward compatibility

The TileCanvas library requires **no changes** - it continues working through MVVM binding as before.