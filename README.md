# Script Launcher

A cross-platform desktop application for triggering scripts via global hotkeys with dynamic, customizable UI popups.

## Features

- **Global Hotkeys**: Trigger scripts from anywhere using customizable keyboard shortcuts
- **Dynamic UI**: Each script defines its own form with custom input fields
- **Multiple Script Types**: Supports PowerShell, Bash, Python, Node.js, Batch files, and executables
- **Cross-Platform**: Built with .NET MAUI for Windows and macOS
- **Extensible**: Add new scripts by simply creating JSON configuration files

## Available Versions

### .NET MAUI Version (Recommended)

Full-featured cross-platform application with dynamic UI generation.

**Location**: `src/ScriptLauncher/`

**Requirements**:
- .NET 8.0 SDK
- Windows 10+ or macOS 13.1+

**Build & Run**:
```bash
cd src/ScriptLauncher
dotnet build
dotnet run
```

### Python Version (Simple)

Lightweight single-purpose tool for Azure DevOps ticket creation.

**Location**: `devops_popup.py`

**Requirements**:
- Python 3.8+

**Run**:
```bash
pip install -r requirements.txt
python devops_popup.py
```

---

## MAUI Version Setup

### Installation

1. Install .NET 8.0 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

2. Clone and build:
   ```bash
   git clone <repository-url>
   cd DevopsPopup
   dotnet build src/ScriptLauncher/ScriptLauncher.csproj
   ```

3. Run the application:
   ```bash
   dotnet run --project src/ScriptLauncher/ScriptLauncher.csproj
   ```

### Configuration

Script configurations are stored in JSON files in the `Configuration/scripts` folder.

#### Default Scripts

| Script | Hotkey | Description |
|--------|--------|-------------|
| DevOps Ticket | `Ctrl+Shift+T` | Create Azure DevOps work items |
| Git Commit | `Ctrl+Shift+G` | Quick git commit with optional push |
| Run Command | `Ctrl+Shift+R` | Execute custom shell commands |

### Creating Custom Scripts

1. Create a new JSON file in `Configuration/scripts/`:

```json
{
  "id": "my-script",
  "name": "My Custom Script",
  "description": "Does something useful",
  "hotkey": {
    "modifiers": ["Ctrl", "Alt"],
    "key": "M"
  },
  "script": {
    "type": "powershell",
    "path": "my-script.ps1",
    "timeout": 30000
  },
  "ui": {
    "title": "My Script",
    "fields": [
      {
        "id": "input1",
        "type": "Text",
        "label": "Input Field",
        "required": true
      },
      {
        "id": "option",
        "type": "Dropdown",
        "label": "Select Option",
        "options": [
          { "value": "a", "label": "Option A" },
          { "value": "b", "label": "Option B" }
        ]
      }
    ]
  },
  "successMessage": "Script completed!"
}
```

2. Create the corresponding script file in the `scripts/` folder.

### Supported Field Types

| Type | Description |
|------|-------------|
| `Text` | Single-line text input |
| `Multiline` | Multi-line text area |
| `Number` | Numeric input |
| `Dropdown` | Select from options |
| `Radio` | Radio button group |
| `Checkbox` | Boolean checkbox |
| `Toggle` | Toggle switch |
| `Date` | Date picker |
| `DateTime` | Date and time picker |
| `Password` | Masked password input |
| `Slider` | Numeric slider |
| `File` | File path input |
| `Folder` | Folder path input |

### Environment Variables

For Azure DevOps integration, set these environment variables:

```bash
AZURE_DEVOPS_ORG=your-organization
AZURE_DEVOPS_PROJECT=your-project
AZURE_DEVOPS_PAT=your-personal-access-token
AZURE_DEVOPS_AREA=Project\Area
AZURE_DEVOPS_ITERATION=Project\Sprint 1
```

---

## Architecture

```
ScriptLauncher/
├── Models/              # Data models for scripts and fields
├── ViewModels/          # MVVM ViewModels
├── Views/               # XAML pages and controls
├── Services/
│   ├── HotkeyService    # Global hotkey handling (SharpHook)
│   ├── ScriptConfigurationService  # Load script JSON configs
│   ├── ScriptExecutionService      # Execute scripts
│   ├── DynamicUIService            # Build UI from field definitions
│   └── WindowService               # Window/popup management
├── Configuration/
│   └── scripts/         # Script definition JSON files
└── Resources/           # Styles, icons, fonts
```

### Key Technologies

- **.NET MAUI 8.0** - Cross-platform UI framework
- **SharpHook** - Global keyboard hooks
- **CommunityToolkit.Mvvm** - MVVM infrastructure
- **System.Text.Json** - Configuration parsing

---

## Creating a Personal Access Token (Azure DevOps)

1. Go to Azure DevOps → User Settings → Personal Access Tokens
2. Click "New Token"
3. Name it (e.g., "Script Launcher")
4. Set expiration as needed
5. Under Scopes, select "Work Items" → "Read & Write"
6. Click "Create" and save the token

---

## Troubleshooting

### macOS: Accessibility Permissions

On macOS, you need to grant Accessibility permissions for global hotkeys to work:

1. Go to System Preferences → Security & Privacy → Privacy
2. Select "Accessibility" from the left panel
3. Add the Script Launcher app to the list

### Windows: Hotkey Conflicts

If hotkeys don't work, check for conflicts with other applications. Try different key combinations in your script configuration.

### Scripts Not Loading

- Ensure JSON files are valid (use a JSON validator)
- Check the app data folder for the `Configuration/scripts` directory
- Review console/debug output for error messages

---

## License

MIT
