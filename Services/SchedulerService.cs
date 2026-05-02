using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using WingetTrayUpdater.Models;

namespace WingetTrayUpdater.Services;

/// <summary>
/// Service for scheduling periodic update checks
/// </summary>
public class SchedulerService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly SettingsService _settingsService;
    private readonly WingetService _wingetService;
    private readonly NotificationService _notificationService;
    private readonly LocalizationService _loc = LocalizationService.Instance;
    
    private Func<Task>? _onCheckDue;
    private bool _disposed;

    public event EventHandler<string>? StatusChanged;
    public event EventHandler? UpdatesFound;

    public SchedulerService(
        SettingsService settingsService,
        WingetService wingetService,
        NotificationService notificationService)
    {
        _settingsService = settingsService;
        _wingetService = wingetService;
        _notificationService = notificationService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timer.Tick += OnTimerTick;

        // Listen for language changes
        _loc.LanguageChanged += (s, e) => { };
    }

    public void SetCheckCallback(Func<Task> callback)
    {
        _onCheckDue = callback;
    }

    public void Start()
    {
        _timer.Start();
        StatusChanged?.Invoke(this, _loc["SchedulerStarted"]);
    }

    public void Stop()
    {
        _timer.Stop();
        StatusChanged?.Invoke(this, _loc["SchedulerStopped"]);
    }

    public void UpdateNextCheckInterval()
    {
        // Handled in timer tick
    }

    public async Task CheckNowAsync()
    {
        if (_onCheckDue != null)
        {
            StatusChanged?.Invoke(this, _loc["Checking"]);
            await _onCheckDue();
        }
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        var settings = _settingsService.Settings;
        var nextCheck = settings.GetNextCheckTime();

        if (DateTime.Now >= nextCheck)
        {
            StatusChanged?.Invoke(this, _loc["Checking"]);
            
            try
            {
                var result = await _wingetService.CheckForUpdatesAsync();
                
                if (result.Success)
                {
                    settings.LastCheckTime = DateTime.Now;
                    settings.LastUpdateCount = result.UpdateCount;
                    settings.Save();

                    if (result.HasUpdates && !settings.SilentMode)
                    {
                        _notificationService.ShowUpdatesAvailable(result.Updates);
                        UpdatesFound?.Invoke(this, EventArgs.Empty);
                    }

                    var nextCheckTime = settings.GetNextCheckTime();
                    if (result.HasUpdates)
                    {
                        StatusChanged?.Invoke(this, 
                            string.Format(_loc["UpdatesFound"], result.UpdateCount) + 
                            ". " + _loc["NextCheck"] + ": " + nextCheckTime.ToString("HH:mm"));
                    }
                    else
                    {
                        StatusChanged?.Invoke(this, 
                            _loc["NoUpdatesAvailable"] + 
                            ". " + _loc["NextCheck"] + ": " + nextCheckTime.ToString("HH:mm"));
                    }
                }
                else if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    StatusChanged?.Invoke(this, _loc["UpdateErrorTitle"] + ": " + result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, _loc["UpdateErrorTitle"] + ": " + ex.Message);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer.Stop();
            _disposed = true;
        }
    }
}