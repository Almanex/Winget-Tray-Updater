using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WingetTrayUpdater.Models;
using WingetTrayUpdater.Services;

namespace WingetTrayUpdater.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly WingetService _wingetService;
    private readonly NotificationService _notificationService;
    private readonly SettingsService _settingsService;
    private readonly SchedulerService _schedulerService;

    [ObservableProperty]
    private string _statusText = "Ready...";

    [ObservableProperty]
    private bool _isChecking;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private bool _hasUpdates;

    [ObservableProperty]
    private int _updateCount;

    [ObservableProperty]
    private ObservableCollection<UpdateInfo> _updates = new();

    [ObservableProperty]
    private bool _autoStartEnabled;

    [ObservableProperty]
    private int _checkIntervalHours = 24;

    [ObservableProperty]
    private bool _silentMode;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private string _wingetVersion = "Checking...";

    private void Log(string msg)
    {
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "winget-tray.log");
            File.AppendAllText(logPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] [VM] " + msg + "\n");
        }
        catch { }
    }

    public MainViewModel(
        WingetService wingetService,
        NotificationService notificationService,
        SettingsService settingsService,
        SchedulerService schedulerService)
    {
        _wingetService = wingetService;
        _notificationService = notificationService;
        _settingsService = settingsService;
        _schedulerService = schedulerService;

        LoadSettings();

        _schedulerService.StatusChanged += (s, status) =>
        {
            StatusText = status;
            Log("Scheduler: " + status);
        };
        _schedulerService.UpdatesFound += (s, e) =>
        {
            Log("Updates found event fired");
        };
    }

    public async Task InitializeAsync()
    {
        var loc = LocalizationService.Instance;
        Log("InitializeAsync started");
        
        try
        {
            StatusText = loc["CheckingWinget"];
            Log("Checking winget availability...");
            
            var available = await _wingetService.IsWingetAvailableAsync();
            WingetVersion = available ? loc["WingetAvailable"] : loc["WingetNotFound"];

            Log("Winget available: " + available);

            if (!available)
            {
                StatusText = loc["WingetNotFound"];
                Log("Winget not available - returning");
                return;
            }

            Log("Waiting before first check...");
            await Task.Delay(2000);
            
            Log("Running initial check...");
            await CheckForUpdatesInternalAsync();
            
            _schedulerService.Start();
            StatusText = string.Format(loc["AutoCheckEnabled"], CheckIntervalHours);
            Log("Scheduler started. Status: " + StatusText);
        }
        catch (Exception ex)
        {
            Log("Init exception: " + ex.Message);
            StatusText = loc["StartupError"] + ": " + ex.Message;
        }
    }

    private void LoadSettings()
    {
        var loc = LocalizationService.Instance;
        var settings = _settingsService.Settings;
        CheckIntervalHours = settings.CheckIntervalHours;
        SilentMode = settings.SilentMode;
        ShowNotifications = settings.ShowNotifications;
        AutoStartEnabled = _settingsService.IsAutoStartEnabled();
        loc.CurrentLanguage = settings.Language;
        Log("Settings loaded: Interval=" + CheckIntervalHours + "h, Silent=" + SilentMode + ", Lang=" + settings.Language);
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        Log("CheckForUpdates command executed");
        await CheckForUpdatesInternalAsync();
    }

    private async Task CheckForUpdatesInternalAsync()
    {
        if (IsChecking)
        {
            Log("Already checking - skipping");
            return;
        }

        var loc = LocalizationService.Instance;

        try
        {
            IsChecking = true;
            StatusText = loc["Checking"];
            Log("Checking for updates...");

            var result = await _wingetService.CheckForUpdatesAsync();

            Log("Check complete: Success=" + result.Success + ", Count=" + result.UpdateCount);

            if (result.Success)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Updates.Clear();
                    foreach (var u in result.Updates)
                        Updates.Add(u);
                    UpdateCount = result.UpdateCount;
                    HasUpdates = result.UpdateCount > 0;
                });

                StatusText = result.HasUpdates
                    ? string.Format(loc["UpdatesFound"], result.UpdateCount)
                    : loc["NoUpdatesAvailable"];

                if (result.HasUpdates && ShowNotifications && !SilentMode)
                {
                    Log("Showing notification for updates");
                    _notificationService.ShowUpdatesAvailable(result.Updates);
                }
            }
            else
            {
                StatusText = loc["UpdateErrorTitle"] + ": " + result.ErrorMessage;
                Log("Check failed: " + result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Log("Check exception: " + ex.Message);
            StatusText = loc["UpdateErrorTitle"] + ": " + ex.Message;
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    private async Task UpdateAllAsync()
    {
        if (IsUpdating || !HasUpdates)
        {
            Log("UpdateAll skipped: IsUpdating=" + IsUpdating + ", HasUpdates=" + HasUpdates);
            return;
        }

        var loc = LocalizationService.Instance;

        try
        {
            IsUpdating = true;
            StatusText = loc["UpdateFailed"] + "...";
            Log("Starting update all...");

            var progress = new Progress<string>(msg =>
            {
                StatusText = msg;
                Log("Progress: " + msg);
            });
            
            var result = await _wingetService.UpgradeAllAsync(progress);

            if (result.Success)
            {
                StatusText = loc["UpdateComplete"];
                Log("Update completed successfully");
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Updates.Clear();
                    HasUpdates = false;
                    UpdateCount = 0;
                });

                if (ShowNotifications)
                {
                    _notificationService.ShowUpdateComplete(UpdateCount, true);
                }
            }
            else
            {
                StatusText = loc["UpdateErrorTitle"] + ": " + result.Output;
                Log("Update failed: " + result.Output);
                _notificationService.ShowError(loc["UpdateErrorTitle"], result.Output);
            }
        }
        catch (Exception ex)
        {
            Log("Update exception: " + ex.Message);
            StatusText = loc["UpdateErrorTitle"] + ": " + ex.Message;
        }
        finally
        {
            IsUpdating = false;
        }
    }

    [RelayCommand]
    private void ToggleAutoStart()
    {
        var loc = LocalizationService.Instance;
        Log("ToggleAutoStart called");
        _settingsService.SetAutoStart(!AutoStartEnabled);
        AutoStartEnabled = !AutoStartEnabled;
        StatusText = AutoStartEnabled ? loc["AutoStartEnabled"] : loc["AutoStartDisabled"];
    }

    [RelayCommand]
    private void ToggleSilentMode()
    {
        var loc = LocalizationService.Instance;
        Log("ToggleSilentMode called");
        _settingsService.UpdateSettings(s => s.SilentMode = !SilentMode);
        SilentMode = !SilentMode;
        StatusText = SilentMode ? loc["SilentModeEnabled"] : loc["SilentModeDisabled"];
    }

    [RelayCommand]
    private void ApplySettings()
    {
        var loc = LocalizationService.Instance;
        Log("ApplySettings called");
        _settingsService.UpdateSettings(s =>
        {
            s.CheckIntervalHours = CheckIntervalHours;
            s.SilentMode = SilentMode;
            s.ShowNotifications = ShowNotifications;
        });
        StatusText = loc["SettingsSaved"];
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (File.Exists(path))
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + path + "\"");
        }
    }

    public void SetLanguage(string language)
    {
        _settingsService.UpdateSettings(s => s.Language = language);
    }
}