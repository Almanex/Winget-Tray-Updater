using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WingetTrayUpdater.Models;

/// <summary>
/// Application settings stored in settings.json next to the executable
/// </summary>
public class AppSettings
{
    // Check interval in hours (default: 24 = once per day)
    public int CheckIntervalHours { get; set; } = 24;
    
    // Start with Windows
    public bool AutoStartWithWindows { get; set; } = false;
    
    // Silent mode - don't show notifications
    public bool SilentMode { get; set; } = false;
    
    // Show Windows notifications
    public bool ShowNotifications { get; set; } = true;
    
    // Run minimized to tray on startup
    public bool RunMinimized { get; set; } = true;
    
    // Language (en, ru, de)
    public string Language { get; set; } = "en";
    
    // Last check timestamp
    public DateTime LastCheckTime { get; set; } = DateTime.MinValue;
    
    // Last update check result summary
    public int LastUpdateCount { get; set; } = 0;
    
    // Path to save settings
    private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "settings.json");

    /// <summary>
    /// Load settings from disk or return defaults
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
        return new AppSettings();
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Get next scheduled check time
    /// </summary>
    public DateTime GetNextCheckTime()
    {
        return LastCheckTime == DateTime.MinValue 
            ? DateTime.Now.AddMinutes(5) // First check after 5 minutes
            : LastCheckTime.AddHours(CheckIntervalHours);
    }
}