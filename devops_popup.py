#!/usr/bin/env python3
"""
Azure DevOps Quick Ticket Creator
A popup application for quickly creating Azure DevOps work items via hotkey.
"""

import json
import base64
import sys
import threading
import tkinter as tk
from tkinter import ttk, messagebox
from pathlib import Path

import requests
from pynput import keyboard

# Windows API imports for focus management
if sys.platform == 'win32':
    import ctypes


class Config:
    """Configuration manager for Azure DevOps settings."""

    def __init__(self, config_path: str = None):
        if config_path is None:
            config_path = Path(__file__).parent / "config.json"

        self.config_path = Path(config_path)
        self._load_config()

    def _load_config(self):
        if not self.config_path.exists():
            raise FileNotFoundError(
                f"Config file not found: {self.config_path}\n"
                "Please copy config.example.json to config.json and fill in your settings."
            )

        with open(self.config_path, 'r') as f:
            config = json.load(f)

        self.organization = config['organization']
        self.project = config['project']
        self.pat = config['personal_access_token']
        self.work_item_type = config.get('work_item_type', 'Task')
        self.area_path = config.get('area_path', '')
        self.iteration_path = config.get('iteration_path', '')
        self.parent_id = config.get('parent_id')
        self.hotkey = config.get('hotkey', '<ctrl>+<shift>+t')


class AzureDevOpsClient:
    """Client for Azure DevOps REST API."""

    def __init__(self, config: Config):
        self.config = config
        self.base_url = f"https://dev.azure.com/{config.organization}/{config.project}/_apis"

        # Create auth header
        credentials = base64.b64encode(f":{config.pat}".encode()).decode()
        self.headers = {
            "Authorization": f"Basic {credentials}",
            "Content-Type": "application/json-patch+json"
        }

    def create_work_item(self, title: str, description: str) -> dict:
        """Create a new work item in Azure DevOps."""
        url = f"{self.base_url}/wit/workitems/${self.config.work_item_type}?api-version=7.0"

        # Build the patch document
        patch_document = [
            {
                "op": "add",
                "path": "/fields/System.Title",
                "value": title
            }
        ]

        if description:
            patch_document.append({
                "op": "add",
                "path": "/fields/System.Description",
                "value": description
            })

        if self.config.area_path:
            patch_document.append({
                "op": "add",
                "path": "/fields/System.AreaPath",
                "value": self.config.area_path
            })

        if self.config.iteration_path:
            patch_document.append({
                "op": "add",
                "path": "/fields/System.IterationPath",
                "value": self.config.iteration_path
            })

        if self.config.parent_id:
            patch_document.append({
                "op": "add",
                "path": "/relations/-",
                "value": {
                    "rel": "System.LinkTypes.Hierarchy-Reverse",
                    "url": f"{self.base_url}/wit/workitems/{self.config.parent_id}"
                }
            })

        response = requests.post(url, headers=self.headers, json=patch_document)
        response.raise_for_status()
        return response.json()


class DevOpsPopup:
    """Main popup window for creating Azure DevOps tickets."""

    def __init__(self, config: Config):
        self.config = config
        self.client = AzureDevOpsClient(config)
        self.root = None
        self.is_visible = False

    def show(self):
        """Show the popup window."""
        if self.is_visible and self.root:
            self.root.lift()
            self.root.focus_force()
            return

        self.root = tk.Tk()
        self.root.title("Create DevOps Ticket")
        self.is_visible = True

        # Window setup - centered popup
        window_width = 550
        window_height = 450
        screen_width = self.root.winfo_screenwidth()
        screen_height = self.root.winfo_screenheight()
        x = (screen_width - window_width) // 2
        y = (screen_height - window_height) // 2
        self.root.geometry(f"{window_width}x{window_height}+{x}+{y}")

        # Ensure window is mapped before focus operations
        self.root.update_idletasks()

        # Keep on top and remove decorations for cleaner look
        self.root.attributes('-topmost', True)
        self.root.resizable(False, False)

        # Style
        style = ttk.Style()
        style.configure('TLabel', font=('Segoe UI', 10))
        style.configure('TButton', font=('Segoe UI', 10))
        style.configure('Header.TLabel', font=('Segoe UI', 12, 'bold'))

        # Main frame with padding
        main_frame = ttk.Frame(self.root, padding="20")
        main_frame.pack(fill=tk.BOTH, expand=True)

        # Header
        header_label = ttk.Label(main_frame, text="Create Azure DevOps Ticket", style='Header.TLabel')
        header_label.pack(pady=(0, 15))

        # Info label showing predefined settings
        info_text = f"Project: {self.config.project} | Type: {self.config.work_item_type}"
        if self.config.parent_id:
            info_text += f" | Parent: #{self.config.parent_id}"
        info_label = ttk.Label(main_frame, text=info_text, foreground='gray')
        info_label.pack(pady=(0, 10))

        # Title field
        title_frame = ttk.Frame(main_frame)
        title_frame.pack(fill=tk.X, pady=(0, 10))

        ttk.Label(title_frame, text="Title:").pack(anchor=tk.W)
        self.title_entry = ttk.Entry(title_frame, font=('Segoe UI', 10))
        self.title_entry.pack(fill=tk.X, pady=(5, 0))

        # Schedule focus after window is fully initialized (150ms delay for Windows)
        self.root.after(150, self._set_initial_focus)

        # Description field
        desc_frame = ttk.Frame(main_frame)
        desc_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 15))

        ttk.Label(desc_frame, text="Description:").pack(anchor=tk.W)

        # Text widget with scrollbar
        text_frame = ttk.Frame(desc_frame)
        text_frame.pack(fill=tk.BOTH, expand=True, pady=(5, 0))

        self.desc_text = tk.Text(text_frame, font=('Segoe UI', 10), height=6, wrap=tk.WORD)
        scrollbar = ttk.Scrollbar(text_frame, orient=tk.VERTICAL, command=self.desc_text.yview)
        self.desc_text.configure(yscrollcommand=scrollbar.set)

        self.desc_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

        # Status label
        self.status_label = ttk.Label(main_frame, text="", foreground='gray')
        self.status_label.pack(pady=(0, 10))

        # Button frame
        button_frame = ttk.Frame(main_frame)
        button_frame.pack(fill=tk.X)

        cancel_btn = ttk.Button(button_frame, text="Cancel", command=self.cancel, width=12)
        cancel_btn.pack(side=tk.RIGHT, padx=(10, 0))

        self.create_btn = ttk.Button(button_frame, text="Create", command=self.create_ticket, width=12)
        self.create_btn.pack(side=tk.RIGHT)

        # Keyboard bindings
        self.root.bind('<Escape>', lambda e: self.cancel())
        self.root.bind('<Control-Return>', lambda e: self.create_ticket())

        # Handle window close
        self.root.protocol("WM_DELETE_WINDOW", self.cancel)

        self.root.mainloop()

    def create_ticket(self):
        """Create the Azure DevOps ticket."""
        title = self.title_entry.get().strip()
        description = self.desc_text.get("1.0", tk.END).strip()

        if not title:
            messagebox.showwarning("Validation Error", "Title is required.")
            self.title_entry.focus()
            return

        # Disable button and show status
        self.create_btn.configure(state='disabled')
        self.status_label.configure(text="Creating ticket...", foreground='blue')
        self.root.update()

        try:
            result = self.client.create_work_item(title, description)
            work_item_id = result.get('id')

            # Show success
            self.status_label.configure(
                text=f"Ticket #{work_item_id} created successfully!",
                foreground='green'
            )
            self.root.update()

            # Clear fields after short delay
            self.root.after(1500, self._clear_and_ready)

        except requests.exceptions.HTTPError as e:
            error_msg = str(e)
            try:
                error_detail = e.response.json()
                if 'message' in error_detail:
                    error_msg = error_detail['message']
            except:
                pass

            self.status_label.configure(text=f"Error: {error_msg}", foreground='red')
            self.create_btn.configure(state='normal')

        except Exception as e:
            self.status_label.configure(text=f"Error: {str(e)}", foreground='red')
            self.create_btn.configure(state='normal')

    def _clear_and_ready(self):
        """Clear the form and prepare for next entry."""
        self.title_entry.delete(0, tk.END)
        self.desc_text.delete("1.0", tk.END)
        self.status_label.configure(text="", foreground='gray')
        self.create_btn.configure(state='normal')
        self.title_entry.focus()

    def _set_initial_focus(self):
        """Set initial focus to title field after window is ready."""
        self.root.update_idletasks()
        self.root.deiconify()
        self.root.lift()

        # On Windows, use the Windows API to force foreground
        if sys.platform == 'win32':
            try:
                hwnd = ctypes.windll.user32.GetParent(self.root.winfo_id())
                ctypes.windll.user32.SetForegroundWindow(hwnd)
            except Exception:
                pass

        self.root.focus_force()
        self.title_entry.focus_set()

        # Schedule a retry in case the first attempt didn't work
        self.root.after(50, lambda: self.title_entry.focus_set())

    def cancel(self):
        """Close the popup window."""
        if self.root:
            self.is_visible = False
            self.root.destroy()
            self.root = None


class HotkeyListener:
    """Global hotkey listener using pynput."""

    def __init__(self, hotkey: str, callback):
        self.hotkey = hotkey
        self.callback = callback
        self.listener = None

    def start(self):
        """Start listening for the hotkey."""
        self.listener = keyboard.GlobalHotKeys({
            self.hotkey: self._on_hotkey
        })
        self.listener.start()

    def _on_hotkey(self):
        """Handle hotkey press."""
        # Run callback in main thread
        self.callback()

    def stop(self):
        """Stop listening."""
        if self.listener:
            self.listener.stop()


class DevOpsPopupApp:
    """Main application controller."""

    def __init__(self):
        self.config = Config()
        self.popup = None
        self.hotkey_listener = None

    def run(self):
        """Run the application."""
        print(f"DevOps Popup is running!")
        print(f"Press {self.config.hotkey} to open the ticket creation popup.")
        print("Press Ctrl+C to exit.")

        # Setup hotkey listener
        self.hotkey_listener = HotkeyListener(self.config.hotkey, self._show_popup)
        self.hotkey_listener.start()

        try:
            # Keep the main thread alive
            self.hotkey_listener.listener.join()
        except KeyboardInterrupt:
            print("\nExiting...")
            self.hotkey_listener.stop()

    def _show_popup(self):
        """Show the popup window in a new thread."""
        # Create popup in a separate thread to not block hotkey listener
        def show():
            popup = DevOpsPopup(self.config)
            popup.show()

        thread = threading.Thread(target=show, daemon=True)
        thread.start()


def main():
    """Entry point."""
    try:
        app = DevOpsPopupApp()
        app.run()
    except FileNotFoundError as e:
        print(f"Configuration Error: {e}")
        return 1
    except Exception as e:
        print(f"Error: {e}")
        return 1
    return 0


if __name__ == "__main__":
    exit(main())
