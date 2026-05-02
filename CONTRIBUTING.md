# Contributing to WingetTrayUpdater

Thank you for your interest in contributing! 🎉

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the [issue list](https://github.com/Almanex/WingetTrayUpdater/issues) as you might find that we already have a ticket for your issue. When you are creating a bug report, please include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Explain what you expected to happen vs what actually happened**
- **Include screenshots if applicable**
- **Include your Windows version and WingetTrayUpdater version**

### Suggesting Features

Feature requests are welcome! Please:

- **Use a clear and descriptive title**
- **Explain in detail why this feature would be useful**
- **Provide examples of how this feature would be used**
- **Be open to discussion about implementation approaches**

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding style** used in the project
3. **Write meaningful commit messages**
4. **Test your changes** before submitting
5. **Update documentation** if needed
6. **Submit your PR** with a clear description

## Development Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11
- Visual Studio 2022 or VS Code (recommended)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_Almanex/WingetTrayUpdater.git
cd WingetTrayUpdater

# Add upstream remote
git remote add upstream https://github.com/ORIGINAL_OWNER/WingetTrayUpdater.git

# Create a feature branch
git checkout -b feature/your-feature-name

# Make your changes and test
dotnet build
dotnet run

# Build release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### Coding Style

- Use **PascalCase** for public members and types
- Use **camelCase** for private members and variables
- Use **implicit typing** (`var`) when the type is obvious
- Add **XML documentation** for public APIs
- Keep methods **small and focused**
- Use **async/await** for asynchronous operations

## Project Structure

```
WingetTrayUpdater/
├── App.xaml(.cs)           # Application entry point, main windows
├── Models/                 # Data models
│   ├── AppSettings.cs      # Settings model
│   └── UpdateInfo.cs       # Update package model
├── Services/               # Business logic services
│   ├── LocalizationService.cs  # Multi-language support
│   ├── NotificationService.cs  # Windows notifications
│   ├── SchedulerService.cs    # Update check scheduler
│   ├── SettingsService.cs     # Settings persistence
│   └── WingetService.cs       # Winget CLI wrapper
├── ViewModels/             # MVVM ViewModels
│   └── MainViewModel.cs    # Main application ViewModel
├── Assets/                 # Static assets
│   └── icon.ico            # App icon
├── .github/                # GitHub configuration
└── WingetTrayUpdater.csproj
```

## License

By contributing, you agree that your contributions will be licensed under the MIT License.