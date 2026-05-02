# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-01

### Added
- System tray integration with custom icon
- Automatic winget update checks with configurable intervals (1h - 7 days)
- Native Windows toast notifications for update alerts
- "Update All" and "Details" action buttons in notifications
- Updates list window showing package versions
- Settings window with:
  - Language selector (English, Russian, German)
  - Auto-start with Windows option
  - Silent mode toggle
  - Check interval configuration
- Portable mode (settings stored in JSON next to executable)
- Multi-language support with dynamic switching
- Comprehensive logging system
- GitHub Actions CI/CD workflow
- Issue and PR templates

### Features
- Runs silently in system tray
- Double-click tray icon to view updates
- Context menu with quick actions
- Real-time status updates
- Error handling and logging
- Single-file executable deployment

## [0.1.0] - 2024-01-01

### Added
- Initial release
- Basic winget update checking
- Simple tray icon
- Manual check trigger
- English language support

---

## Versioning

We use [Semantic Versioning](https://semver.org/). For available versions, see the [tags on this repository](https://github.com/Almanex/WingetTrayUpdater/tags).

## Releases

Download the latest release from the [Releases page](../../releases/latest).