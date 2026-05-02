using System;

namespace WingetTrayUpdater.Models;

public class UpdateInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public string AvailableVersion { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public override string ToString() => Name + " (" + CurrentVersion + " -> " + AvailableVersion + ")";
}

public class UpgradeCheckResult
{
    public bool Success { get; set; }
    public System.Collections.Generic.List<UpdateInfo> Updates { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; } = DateTime.Now;

    public bool HasUpdates => Updates.Count > 0;
    public int UpdateCount => Updates.Count;
}