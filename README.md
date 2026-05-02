# WingetTrayUpdater

A lightweight Windows tray utility that monitors Winget package updates and displays native Windows notifications when updates are available.

![Preview](Assets/preview.png)

## Features

- **System Tray Integration** - Runs silently in the background with a tray icon
- **Automatic Updates Check** - Scheduled checks (configurable: 1h - 7 days)
- **Native Windows Notifications** - Toast notifications with quick action buttons
- **One-Click Update** - Update all packages with a single click
- **Multi-language Support** - English, Russian, German
- **Portable** - No installation required, settings stored in JSON
- **Auto-start Option** - Run at Windows startup (optional)

## Requirements

- Windows 10 version 1809 or later / Windows 11
- [Winget](https://github.com/microsoft/winget-cli) (pre-installed on Windows 11 and modern Windows 10)

## Installation

### Portable (Recommended)
1. Download the latest `WingetTrayUpdater.exe` from [Releases](https://github.com/YOUR_Almanex/WingetTrayUpdater/releases/latest)
2. Run the executable
3. The app will appear in the system tray

### From Source
```bash
git clone https://github.com/YOUR_Almanex/WingetTrayUpdater.git
cd WingetTrayUpdater
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

## Usage

### Tray Menu
- **Check for updates** - Manually trigger an update check
- **Update all** - Install all available updates
- **Show updates** - View the list of available updates
- **Settings** - Configure the application
- **Exit** - Close the application

### Notifications
When updates are available, you'll receive a Windows toast notification with:
- Number of available updates
- List of packages being updated
- **Update All** button - Start updating immediately
- **Details** button - Open the updates list window

### Settings
- **Language** - Switch between English, Russian, German
- **Run at Windows startup** - Enable/disable auto-start
- **Silent mode** - Disable notifications
- **Check interval** - Set automatic check frequency

## Keyboard Shortcuts

- **Double-click tray icon** - Open updates list
- **Right-click tray icon** - Show context menu

## Configuration

Settings are stored in `settings.json` next to the executable:

```json
{
  "CheckIntervalHours": 24,
  "AutoStartWithWindows": false,
  "SilentMode": false,
  "ShowNotifications": true,
  "RunMinimized": true,
  "Language": "en",
  "LastCheckTime": "2024-01-01T00:00:00",
  "LastUpdateCount": 0
}
```

## Logs

- `winget-tray.log` - Application log
- `error.log` - Error log (if any)

## Building

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

### Build Commands
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### Dependencies
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM framework
- [Microsoft.Toolkit.Uwp.Notifications](https://github.com/notificationtools/microsoft-toolkit-uwp-notifications) - Toast notifications
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) - System tray icon

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Winget](https://github.com/microsoft/winget-cli) - Windows Package Manager
- All contributors who improve this project

---

Made with ❤️ for Windows users who want simple, automatic package updates.