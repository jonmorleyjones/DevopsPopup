# DevOps Popup

A lightweight popup application for quickly creating Azure DevOps work items using a global hotkey.

## Features

- Global hotkey to trigger popup from anywhere
- Simple form with Title and Description fields
- Predefined Area Path, Iteration Path, Project, and Parent work item
- Creates tickets via Azure DevOps REST API
- Confirmation feedback and auto-clear for rapid ticket creation

## Requirements

- Python 3.8+
- Azure DevOps account with a Personal Access Token (PAT)

## Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd DevopsPopup
   ```

2. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

3. Create your configuration file:
   ```bash
   cp config.example.json config.json
   ```

4. Edit `config.json` with your Azure DevOps settings:
   ```json
   {
       "organization": "your-organization",
       "project": "your-project",
       "personal_access_token": "your-pat-token",
       "work_item_type": "Task",
       "area_path": "your-project\\Team",
       "iteration_path": "your-project\\Sprint 1",
       "parent_id": 12345,
       "hotkey": "<ctrl>+<shift>+t"
   }
   ```

## Configuration Options

| Field | Description |
|-------|-------------|
| `organization` | Your Azure DevOps organization name |
| `project` | The project where tickets will be created |
| `personal_access_token` | PAT with Work Items (Read & Write) scope |
| `work_item_type` | Type of work item to create (Task, Bug, User Story, etc.) |
| `area_path` | Predefined area path for new tickets |
| `iteration_path` | Predefined iteration path for new tickets |
| `parent_id` | (Optional) Parent work item ID to link new tickets to |
| `hotkey` | Global hotkey to trigger the popup |

### Hotkey Format

The hotkey uses pynput format:
- `<ctrl>+<shift>+t` - Ctrl + Shift + T
- `<alt>+<shift>+d` - Alt + Shift + D
- `<cmd>+<shift>+t` - Command + Shift + T (macOS)

## Usage

1. Run the application:
   ```bash
   python devops_popup.py
   ```

2. Press your configured hotkey (default: `Ctrl+Shift+T`) to open the popup

3. Enter a title and optional description

4. Click **Create** or press `Ctrl+Enter` to create the ticket

5. Press **Cancel** or `Escape` to close without creating

## Creating a Personal Access Token

1. Go to Azure DevOps > User Settings > Personal Access Tokens
2. Click "New Token"
3. Give it a name (e.g., "DevOps Popup")
4. Set expiration as needed
5. Under Scopes, select "Work Items" > "Read & Write"
6. Click "Create" and copy the token to your config.json

## Running at Startup

### Linux (systemd)

Create `~/.config/systemd/user/devops-popup.service`:
```ini
[Unit]
Description=DevOps Popup

[Service]
ExecStart=/usr/bin/python3 /path/to/devops_popup.py
Restart=always

[Install]
WantedBy=default.target
```

Enable with: `systemctl --user enable --now devops-popup`

### Windows

Add a shortcut to `devops_popup.py` in your Startup folder.

### macOS

Create a Launch Agent or use Automator to run the script at login.

## License

MIT
