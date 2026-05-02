using System;
using System.IO;
using Microsoft.Win32;
using WingetTrayUpdater.Models;

namespace WingetTrayUpdater.Services;

/// <summary>
/// Service for managing application settings and startup behavior
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public AppSettings Settings => _settings;

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        _settings = AppSettings.Load();
    }

    /// <summary>
    /// Reload settings from disk
    /// </summary>
    public void Reload()
    {
        _settings = AppSettings.Load();
    }

    /// <summary>
    /// Save current settings to disk
    /// </summary>
    public void Save()
    {
        _settings.Save();
    }

    /// <summary>
    /// Update settings and save
    /// </summary>
    public void UpdateSettings(Action<AppSettings> updateAction)
    {
        updateAction(_settings);
        _settings.Save();
    }

    /// <summary>
    /// Configure auto-start with Windows via Registry
    /// </summary>
    public void SetAutoStart(bool enable)
    {
        try
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "WingetTrayUpdater";

            using var key = Registry.CurrentUser.OpenSubKey(keyPath, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = AppContext.BaseDirectory + "WingetTrayUpdater.exe";
                key.SetValue(valueName, $"\"{exePath}\" --minimized");
            }
            else
            {
                key.DeleteValue(valueName, false);
            }

            _settings.AutoStartWithWindows = enable;
            _settings.Save();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set auto-start: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if auto-start is enabled
    /// </summary>
    public bool IsAutoStartEnabled()
    {
        try
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "WingetTrayUpdater";

            using var key = Registry.CurrentUser.OpenSubKey(keyPath, false);
            return key?.GetValue(valueName) != null;
        }
        catch
        {
            return false;
        }
    }
}