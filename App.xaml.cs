using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using WingetTrayUpdater.Services;
using WingetTrayUpdater.ViewModels;

namespace WingetTrayUpdater;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainViewModel? _viewModel;
    private SchedulerService? _schedulerService;
    private Window? _settingsWindow;
    private Window? _updatesWindow;
    private LocalizationService? _loc;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogException(args.ExceptionObject as Exception);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogException(args.Exception);
            args.Handled = true;
        };

        bool startMinimized = e.Args.Contains("--minimized");

        try
        {
            var settingsService = new SettingsService();
            var wingetService = new WingetService();
            var notificationService = new NotificationService();
            _schedulerService = new SchedulerService(settingsService, wingetService, notificationService);

            // Initialize localization
            _loc = LocalizationService.Instance;
            _loc.CurrentLanguage = settingsService.Settings.Language;
            _loc.LanguageChanged += (s, args) => RefreshUI();

            _viewModel = new MainViewModel(wingetService, notificationService, settingsService, _schedulerService);

            _schedulerService.SetCheckCallback(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.CheckForUpdatesCommand.ExecuteAsync(null);
                }
            });

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                var args = ToastArguments.Parse(toastArgs.Argument);
                
                if (args.TryGetValue("action", out string action))
                {
                    Dispatcher.Invoke(() =>
                    {
                        switch (action)
                        {
                            case "updateAll":
                                _viewModel?.UpdateAllCommand.Execute(null);
                                break;
                            case "showDetails":
                                ShowUpdatesWindow();
                                break;
                        }
                    });
                }
            };

            CreateTrayIcon();
            _settingsWindow = CreateSettingsWindow();

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                await Dispatcher.InvokeAsync(async () => await _viewModel!.InitializeAsync());
            });

            _viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.StatusText) || 
                    args.PropertyName == nameof(MainViewModel.UpdateCount))
                {
                    UpdateTrayTooltip();
                }
            };

            if (!startMinimized)
            {
                ShowSettingsWindow();
            }

            Log(_loc["AppStarted"]);
        }
        catch (Exception ex)
        {
            LogException(ex);
            MessageBox.Show(_loc["StartupError"] + ": " + ex.Message, _loc["AppName"], 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void RefreshUI()
    {
        CreateTrayIcon();
        _settingsWindow?.Close();
        _settingsWindow = CreateSettingsWindow();
        if (_updatesWindow != null)
        {
            _updatesWindow.Close();
            _updatesWindow = null;
        }
    }

    private void CreateTrayIcon()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Dispose();
        }

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = _loc?["AppName"] ?? "WingetTrayUpdater",
            Visibility = Visibility.Visible
        };

        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "icon.ico");
            if (File.Exists(iconPath))
            {
                _trayIcon.Icon = new Icon(iconPath);
            }
            else
            {
                _trayIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            _trayIcon.Icon = SystemIcons.Application;
        }

        var contextMenu = new ContextMenu();

        var header = new MenuItem { Header = _loc?["AppName"] ?? "WingetTrayUpdater", IsEnabled = false, FontWeight = FontWeights.Bold };
        var separator1 = new Separator();
        
        var checkItem = new MenuItem { Header = _loc?["CheckForUpdates"] ?? "Check for updates" };
        checkItem.Click += async (s, e) =>
        {
            if (_viewModel != null) await _viewModel.CheckForUpdatesCommand.ExecuteAsync(null);
        };

        var updateAllItem = new MenuItem { Header = _loc?["UpdateAll"] ?? "Update all" };
        updateAllItem.Click += async (s, e) =>
        {
            if (_viewModel != null) await _viewModel.UpdateAllCommand.ExecuteAsync(null);
        };

        var showUpdatesItem = new MenuItem { Header = _loc?["ShowUpdates"] ?? "Show updates" };
        showUpdatesItem.Click += (s, e) => ShowUpdatesWindow();

        var separator2 = new Separator();
        var settingsItem = new MenuItem { Header = _loc?["Settings"] ?? "Settings" };
        settingsItem.Click += (s, e) => ShowSettingsWindow();

        var separator3 = new Separator();
        var exitItem = new MenuItem { Header = _loc?["Exit"] ?? "Exit" };
        exitItem.Click += (s, e) => ExitApplication();

        contextMenu.Items.Add(header);
        contextMenu.Items.Add(separator1);
        contextMenu.Items.Add(checkItem);
        contextMenu.Items.Add(updateAllItem);
        contextMenu.Items.Add(showUpdatesItem);
        contextMenu.Items.Add(separator2);
        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(separator3);
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
        _trayIcon.TrayMouseDoubleClick += (s, e) => ShowUpdatesWindow();
    }

    private Window CreateSettingsWindow()
    {
        var window = new Window
        {
            Title = _loc?["SettingsTitle"] ?? "Settings",
            Width = 400,
            Height = 340,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new Grid { Margin = new Thickness(20) };
        for (int i = 0; i < 6; i++) grid.RowDefinitions.Add(new RowDefinition { Height = i < 5 ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });

        var header = new TextBlock { Text = _loc?["SettingsTitle"] ?? "Settings", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 16) };
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        // Language selector
        var langPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        langPanel.Children.Add(new TextBlock { Text = "Language:", VerticalAlignment = VerticalAlignment.Center });
        var langCombo = new ComboBox { Width = 120, Margin = new Thickness(8, 0, 0, 0) };
        foreach (var lang in _loc?.AvailableLanguages ?? new[] { "en", "ru", "de" })
        {
            langCombo.Items.Add(new ComboBoxItem { Content = _loc?.GetLanguageDisplayName(lang) ?? lang, Tag = lang });
        }
        langCombo.SelectedIndex = Array.IndexOf(_loc?.AvailableLanguages ?? new[] { "en" }, _loc?.CurrentLanguage ?? "en");
        langCombo.SelectionChanged += (s, ev) =>
        {
            if (langCombo.SelectedItem is ComboBoxItem item && item.Tag is string lang && _loc != null)
            {
                _loc.CurrentLanguage = lang;
                _viewModel?.SetLanguage(lang);
            }
        };
        langPanel.Children.Add(langCombo);
        Grid.SetRow(langPanel, 1);
        grid.Children.Add(langPanel);

        var checksPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        var autoStartCheck = new CheckBox { Content = _loc?["RunAtStartup"] ?? "Run at Windows startup", Margin = new Thickness(0, 4, 0, 0) };
        autoStartCheck.Click += (s, ev) => _viewModel?.ToggleAutoStartCommand.Execute(null);
        var silentCheck = new CheckBox { Content = _loc?["SilentMode"] ?? "Silent mode", Margin = new Thickness(0, 4, 0, 0) };
        silentCheck.Click += (s, ev) => _viewModel?.ToggleSilentModeCommand.Execute(null);
        checksPanel.Children.Add(autoStartCheck);
        checksPanel.Children.Add(silentCheck);
        Grid.SetRow(checksPanel, 2);
        grid.Children.Add(checksPanel);

        var intervalPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        intervalPanel.Children.Add(new TextBlock { Text = _loc?["CheckInterval"] ?? "Check interval:", VerticalAlignment = VerticalAlignment.Center });
        var intervalCombo = new ComboBox { Width = 140, Margin = new Thickness(8, 0, 0, 0) };
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["1Hour"] ?? "1 hour", Tag = 1 });
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["6Hours"] ?? "6 hours", Tag = 6 });
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["12Hours"] ?? "12 hours", Tag = 12 });
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["24Hours"] ?? "24 hours", Tag = 24 });
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["3Days"] ?? "3 days", Tag = 72 });
        intervalCombo.Items.Add(new ComboBoxItem { Content = _loc?["7Days"] ?? "7 days", Tag = 168 });
        intervalCombo.SelectedIndex = 3;
        intervalCombo.SelectionChanged += (s, ev) =>
        {
            if (intervalCombo.SelectedItem is ComboBoxItem item && item.Tag is int hours && _viewModel != null)
            {
                _viewModel.CheckIntervalHours = hours;
                _viewModel.ApplySettingsCommand.Execute(null);
            }
        };
        intervalPanel.Children.Add(intervalCombo);
        Grid.SetRow(intervalPanel, 3);
        grid.Children.Add(intervalPanel);

        var statusText = new TextBlock { Text = _loc?["Status"] + ": " + (_viewModel?.StatusText ?? _loc?["Ready"] ?? "Ready"), Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(0, 0, 0, 12) };
        Grid.SetRow(statusText, 4);
        grid.Children.Add(statusText);

        _viewModel!.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.StatusText))
            {
                statusText.Text = (_loc?["Status"] ?? "Status") + ": " + _viewModel.StatusText;
            }
        };

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var checkButton = new Button { Content = _loc?["CheckForUpdates"] ?? "Check", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 0) };
        checkButton.Click += async (s, ev) =>
        {
            if (_viewModel != null) await _viewModel.CheckForUpdatesCommand.ExecuteAsync(null);
        };
        var updateButton = new Button { Content = _loc?["UpdateAll"] ?? "Update All", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 0, 0) };
        updateButton.Click += async (s, ev) =>
        {
            if (_viewModel != null) await _viewModel.UpdateAllCommand.ExecuteAsync(null);
        };
        buttonPanel.Children.Add(checkButton);
        buttonPanel.Children.Add(updateButton);
        Grid.SetRow(buttonPanel, 5);
        grid.Children.Add(buttonPanel);

        window.Content = grid;
        window.Closing += (s, ev) => { ev.Cancel = true; window.Hide(); };
        
        return window;
    }

    private void ShowUpdatesWindow()
    {
        if (_viewModel == null) return;

        if (_updatesWindow != null && _updatesWindow.IsVisible)
        {
            _updatesWindow.Activate();
            return;
        }

        var window = new Window
        {
            Title = _loc?["UpdatesTitle"] ?? "Available Updates",
            Width = 500,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        var grid = new Grid { Margin = new Thickness(16) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new TextBlock { Text = _loc?["UpdatesTitle"] ?? "Available Updates", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 12) };
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        var listBox = new ListBox { Margin = new Thickness(0, 0, 0, 12) };
        
        var itemTemplate = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(StackPanel));
        factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        factory.SetValue(StackPanel.MarginProperty, new Thickness(4));
        
        var nameBlock = new FrameworkElementFactory(typeof(TextBlock));
        nameBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Name"));
        nameBlock.SetValue(TextBlock.WidthProperty, 200.0);
        factory.AppendChild(nameBlock);

        var versionPanel = new FrameworkElementFactory(typeof(StackPanel));
        versionPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        
        var currentVer = new FrameworkElementFactory(typeof(TextBlock));
        currentVer.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("CurrentVersion"));
        currentVer.SetValue(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Gray);
        
        var arrow = new FrameworkElementFactory(typeof(TextBlock));
        arrow.SetValue(TextBlock.TextProperty, " -> ");
        arrow.SetValue(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Green);
        
        var newVer = new FrameworkElementFactory(typeof(TextBlock));
        newVer.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("AvailableVersion"));
        newVer.SetValue(TextBlock.ForegroundProperty, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 197, 94)));
        newVer.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);

        versionPanel.AppendChild(currentVer);
        versionPanel.AppendChild(arrow);
        versionPanel.AppendChild(newVer);
        factory.AppendChild(versionPanel);

        itemTemplate.VisualTree = factory;
        listBox.ItemTemplate = itemTemplate;
        listBox.ItemsSource = _viewModel.Updates;

        Grid.SetRow(listBox, 1);
        grid.Children.Add(listBox);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var refreshButton = new Button { Content = _loc?["Refresh"] ?? "Refresh", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 0) };
        refreshButton.Click += async (s, ev) =>
        {
            if (_viewModel != null) await _viewModel.CheckForUpdatesCommand.ExecuteAsync(null);
        };
        var updateButton = new Button { Content = _loc?["UpdateAll"] ?? "Update All", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 0, 0) };
        updateButton.Click += async (s, ev) =>
        {
            if (_viewModel != null) await _viewModel.UpdateAllCommand.ExecuteAsync(null);
        };
        buttonPanel.Children.Add(refreshButton);
        buttonPanel.Children.Add(updateButton);
        Grid.SetRow(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        window.Content = grid;
        window.Closed += (s, ev) => { _updatesWindow = null; };
        
        _updatesWindow = window;
        window.Show();
    }

    private void ShowSettingsWindow()
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Show();
            _settingsWindow.WindowState = WindowState.Normal;
            _settingsWindow.Activate();
            _settingsWindow.Focus();
        }
    }

    private void UpdateTrayTooltip()
    {
        if (_trayIcon != null && _viewModel != null)
        {
            var count = _viewModel.UpdateCount;
            if (count > 0)
            {
                _trayIcon.ToolTipText = string.Format(_loc?["TrayTooltipWithUpdates"] ?? "WingetTrayUpdater\n{0}\n({1} updates)", 
                    _viewModel.StatusText, count);
            }
            else
            {
                _trayIcon.ToolTipText = string.Format(_loc?["TrayTooltipNoUpdates"] ?? "WingetTrayUpdater\n{0}", 
                    _viewModel.StatusText);
            }
        }
    }

    private void ExitApplication()
    {
        _schedulerService?.Dispose();
        _trayIcon?.Dispose();
        _updatesWindow?.Close();
        _settingsWindow?.Close();
        Shutdown();
    }

    private void Log(string message)
    {
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "winget-tray.log");
            File.AppendAllText(logPath, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message + "\n");
        }
        catch { }
    }

    private void LogException(Exception? ex)
    {
        if (ex == null) return;
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
            File.AppendAllText(logPath, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + ex.Message + "\n" + ex.StackTrace + "\n\n");
        }
        catch { }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _schedulerService?.Dispose();
        base.OnExit(e);
    }
}