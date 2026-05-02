using System;
using System.Collections.Generic;
using System.Globalization;

namespace WingetTrayUpdater.Services;

public class LocalizationService
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private string _currentLanguage = "en";
    
    public event EventHandler? LanguageChanged;

    private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            // App
            ["AppName"] = "WingetTrayUpdater",
            ["AppStarted"] = "App started successfully",
            ["StartupError"] = "Startup error",
            
            // Tray Menu
            ["CheckForUpdates"] = "Check for updates",
            ["UpdateAll"] = "Update all",
            ["ShowUpdates"] = "Show updates",
            ["Settings"] = "Settings",
            ["Exit"] = "Exit",
            
            // Settings Window
            ["SettingsTitle"] = "Settings",
            ["Status"] = "Status",
            ["RunAtStartup"] = "Run at Windows startup",
            ["SilentMode"] = "Silent mode (no notifications)",
            ["CheckInterval"] = "Check interval",
            ["Check"] = "Check",
            
            // Updates Window
            ["UpdatesTitle"] = "Available Updates",
            ["Package"] = "Package",
            ["Version"] = "Version",
            ["Current"] = "Current",
            ["Available"] = "Available",
            ["NoUpdates"] = "No updates available",
            ["AllUpToDate"] = "All packages up to date",
            ["Refresh"] = "Refresh",
            
            // Status Messages
            ["Ready"] = "Ready",
            ["Checking"] = "Checking for updates...",
            ["CheckingWinget"] = "Checking Winget...",
            ["WingetAvailable"] = "Winget available",
            ["WingetNotFound"] = "Winget not found!",
            ["UpdatesFound"] = "{0} updates available",
            ["NoUpdatesAvailable"] = "All packages up to date",
            ["UpdateComplete"] = "Update completed!",
            ["UpdateFailed"] = "Update failed",
            ["AutoCheckEnabled"] = "Auto-check every {0} hours",
            ["SchedulerStarted"] = "Scheduler started",
            ["SchedulerStopped"] = "Scheduler stopped",
            ["NextCheck"] = "Next check",
            
            // Notifications
            ["UpdatesNotificationTitle"] = "{0} update(s) available",
            ["UpdateAllButton"] = "Update All",
            ["DetailsButton"] = "Details",
            ["UpdateCompleteTitle"] = "Update completed",
            ["UpdateSuccessMsg"] = "Successfully updated {0} packages",
            ["UpdateErrorTitle"] = "Update error",
            
            // Intervals
            ["1Hour"] = "1 hour",
            ["6Hours"] = "6 hours",
            ["12Hours"] = "12 hours",
            ["24Hours"] = "24 hours",
            ["3Days"] = "3 days",
            ["7Days"] = "7 days",
            
            // Tray Tooltip
            ["TrayTooltipNoUpdates"] = "WingetTrayUpdater\n{0}",
            ["TrayTooltipWithUpdates"] = "WingetTrayUpdater\n{0}\n({1} updates)",
            
            // Settings saved
            ["SettingsSaved"] = "Settings saved",
            ["AutoStartEnabled"] = "Auto-start enabled",
            ["AutoStartDisabled"] = "Auto-start disabled",
            ["SilentModeEnabled"] = "Silent mode enabled",
            ["SilentModeDisabled"] = "Silent mode disabled",
        },
        
        ["ru"] = new Dictionary<string, string>
        {
            // App
            ["AppName"] = "WingetTrayUpdater",
            ["AppStarted"] = "Приложение успешно запущено",
            ["StartupError"] = "Ошибка запуска",
            
            // Tray Menu
            ["CheckForUpdates"] = "Проверить обновления",
            ["UpdateAll"] = "Обновить всё",
            ["ShowUpdates"] = "Показать обновления",
            ["Settings"] = "Настройки",
            ["Exit"] = "Выход",
            
            // Settings Window
            ["SettingsTitle"] = "Настройки",
            ["Status"] = "Статус",
            ["RunAtStartup"] = "Запускать с Windows",
            ["SilentMode"] = "Тихий режим (без уведомлений)",
            ["CheckInterval"] = "Интервал проверки",
            ["Check"] = "Проверить",
            
            // Updates Window
            ["UpdatesTitle"] = "Доступные обновления",
            ["Package"] = "Пакет",
            ["Version"] = "Версия",
            ["Current"] = "Текущая",
            ["Available"] = "Доступная",
            ["NoUpdates"] = "Нет доступных обновлений",
            ["AllUpToDate"] = "Все пакеты актуальны",
            ["Refresh"] = "Обновить",
            
            // Status Messages
            ["Ready"] = "Готов",
            ["Checking"] = "Проверка обновлений...",
            ["CheckingWinget"] = "Проверка Winget...",
            ["WingetAvailable"] = "Winget доступен",
            ["WingetNotFound"] = "Winget не найден!",
            ["UpdatesFound"] = "Доступно обновлений: {0}",
            ["NoUpdatesAvailable"] = "Все пакеты актуальны",
            ["UpdateComplete"] = "Обновление завершено!",
            ["UpdateFailed"] = "Ошибка обновления",
            ["AutoCheckEnabled"] = "Автопроверка каждые {0} ч.",
            ["SchedulerStarted"] = "Планировщик запущен",
            ["SchedulerStopped"] = "Планировщик остановлен",
            ["NextCheck"] = "Следующая проверка",
            
            // Notifications
            ["UpdatesNotificationTitle"] = "Доступно обновлений: {0}",
            ["UpdateAllButton"] = "Обновить всё",
            ["DetailsButton"] = "Подробнее",
            ["UpdateCompleteTitle"] = "Обновление завершено",
            ["UpdateSuccessMsg"] = "Успешно обновлено пакетов: {0}",
            ["UpdateErrorTitle"] = "Ошибка обновления",
            
            // Intervals
            ["1Hour"] = "1 час",
            ["6Hours"] = "6 часов",
            ["12Hours"] = "12 часов",
            ["24Hours"] = "24 часа",
            ["3Days"] = "3 дня",
            ["7Days"] = "7 дней",
            
            // Tray Tooltip
            ["TrayTooltipNoUpdates"] = "WingetTrayUpdater\n{0}",
            ["TrayTooltipWithUpdates"] = "WingetTrayUpdater\n{0}\n({1} обновлений)",
            
            // Settings saved
            ["SettingsSaved"] = "Настройки сохранены",
            ["AutoStartEnabled"] = "Автозапуск включен",
            ["AutoStartDisabled"] = "Автозапуск выключен",
            ["SilentModeEnabled"] = "Тихий режим включен",
            ["SilentModeDisabled"] = "Тихий режим выключен",
        },
        
        ["de"] = new Dictionary<string, string>
        {
            // App
            ["AppName"] = "WingetTrayUpdater",
            ["AppStarted"] = "Anwendung erfolgreich gestartet",
            ["StartupError"] = "Startfehler",
            
            // Tray Menu
            ["CheckForUpdates"] = "Nach Updates suchen",
            ["UpdateAll"] = "Alle aktualisieren",
            ["ShowUpdates"] = "Updates anzeigen",
            ["Settings"] = "Einstellungen",
            ["Exit"] = "Beenden",
            
            // Settings Window
            ["SettingsTitle"] = "Einstellungen",
            ["Status"] = "Status",
            ["RunAtStartup"] = "Mit Windows starten",
            ["SilentMode"] = "Stiller Modus (keine Benachrichtigungen)",
            ["CheckInterval"] = "Pruefintervall",
            ["Check"] = "Pruefen",
            
            // Updates Window
            ["UpdatesTitle"] = "Verfuegbare Updates",
            ["Package"] = "Paket",
            ["Version"] = "Version",
            ["Current"] = "Aktuell",
            ["Available"] = "Verfuegbar",
            ["NoUpdates"] = "Keine Updates verfuegbar",
            ["AllUpToDate"] = "Alle Pakete sind aktuell",
            ["Refresh"] = "Aktualisieren",
            
            // Status Messages
            ["Ready"] = "Bereit",
            ["Checking"] = "Suche nach Updates...",
            ["CheckingWinget"] = "Pruefe Winget...",
            ["WingetAvailable"] = "Winget verfuegbar",
            ["WingetNotFound"] = "Winget nicht gefunden!",
            ["UpdatesFound"] = "{0} Updates verfuegbar",
            ["NoUpdatesAvailable"] = "Alle Pakete sind aktuell",
            ["UpdateComplete"] = "Aktualisierung abgeschlossen!",
            ["UpdateFailed"] = "Aktualisierung fehlgeschlagen",
            ["AutoCheckEnabled"] = "Automatische Pruefung alle {0} Stunden",
            ["SchedulerStarted"] = "Zeitplaner gestartet",
            ["SchedulerStopped"] = "Zeitplaner gestoppt",
            ["NextCheck"] = "Naechste Pruefung",
            
            // Notifications
            ["UpdatesNotificationTitle"] = "{0} Update(s) verfuegbar",
            ["UpdateAllButton"] = "Alle aktualisieren",
            ["DetailsButton"] = "Details",
            ["UpdateCompleteTitle"] = "Aktualisierung abgeschlossen",
            ["UpdateSuccessMsg"] = "{0} Pakete erfolgreich aktualisiert",
            ["UpdateErrorTitle"] = "Aktualisierungsfehler",
            
            // Intervals
            ["1Hour"] = "1 Stunde",
            ["6Hours"] = "6 Stunden",
            ["12Hours"] = "12 Stunden",
            ["24Hours"] = "24 Stunden",
            ["3Days"] = "3 Tage",
            ["7Days"] = "7 Tage",
            
            // Tray Tooltip
            ["TrayTooltipNoUpdates"] = "WingetTrayUpdater\n{0}",
            ["TrayTooltipWithUpdates"] = "WingetTrayUpdater\n{0}\n({1} Updates)",
            
            // Settings saved
            ["SettingsSaved"] = "Einstellungen gespeichert",
            ["AutoStartEnabled"] = "Autostart aktiviert",
            ["AutoStartDisabled"] = "Autostart deaktiviert",
            ["SilentModeEnabled"] = "Stiller Modus aktiviert",
            ["SilentModeDisabled"] = "Stiller Modus deaktiviert",
        }
    };

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value && _translations.ContainsKey(value))
            {
                _currentLanguage = value;
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string[] AvailableLanguages => new[] { "en", "ru", "de" };

    public string GetLanguageDisplayName(string lang)
    {
        return lang switch
        {
            "en" => "English",
            "ru" => "Русский",
            "de" => "Deutsch",
            _ => lang
        };
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        // Fallback to English
        if (_translations.TryGetValue("en", out var enDict))
        {
            if (enDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        
        return key;
    }

    public string Get(string key, params object[] args)
    {
        var template = Get(key);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    public void SetLanguageFromSystem()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (_translations.ContainsKey(culture))
        {
            CurrentLanguage = culture;
        }
        else
        {
            CurrentLanguage = "en";
        }
    }
}