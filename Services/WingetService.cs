using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingetTrayUpdater.Models;

namespace WingetTrayUpdater.Services;

/// <summary>
/// Service for interacting with Windows Package Manager (winget)
/// </summary>
public class WingetService
{
    private readonly string _wingetPath;

    public WingetService()
    {
        // Try to find winget.exe in standard locations
        _wingetPath = FindWingetPath();
    }

    private string FindWingetPath()
    {
        // Common locations for winget
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "WindowsApps", "winget.exe"),
            "winget.exe" // Rely on PATH
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path) || path == "winget.exe")
            {
                return path;
            }
        }

        return "winget.exe"; // Default to PATH resolution
    }

    /// <summary>
    /// Check if winget is available
    /// </summary>
    public async Task<bool> IsWingetAvailableAsync()
    {
        try
        {
            var result = await RunWingetCommandAsync("--version");
            return result.Success && !string.IsNullOrWhiteSpace(result.Output);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check for available updates
    /// </summary>
    public async Task<UpgradeCheckResult> CheckForUpdatesAsync()
    {
        var result = new UpgradeCheckResult
        {
            CheckedAt = DateTime.Now
        };

        try
        {
            var output = await RunWingetCommandAsync("upgrade --include-unknown");
            
            if (!output.Success)
            {
                result.ErrorMessage = output.Error;
                return result;
            }

            result.Updates = ParseUpgradeList(output.Output);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Upgrade all available packages
    /// </summary>
    public async Task<(bool Success, string Output)> UpgradeAllAsync(IProgress<string>? progress = null)
    {
        progress?.Report("Starting winget upgrade...");
        
        try
        {
            // Run winget upgrade --all
            var result = await RunWingetCommandAsync("upgrade --all --accept-package-agreements --accept-source-agreements");
            
            if (result.Success)
            {
                progress?.Report("Updates completed successfully!");
            }
            else
            {
                progress?.Report($"Update failed: {result.Error}");
            }

            return (result.Success, result.Output);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Upgrade specific package by ID
    /// </summary>
    public async Task<(bool Success, string Output)> UpgradePackageAsync(string packageId)
    {
        try
        {
            var result = await RunWingetCommandAsync(
                $"upgrade --id {packageId} --accept-package-agreements --accept-source-agreements");
            return (result.Success, result.Output);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<(bool Success, string Output, string Error)> RunWingetCommandAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _wingetPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return (process.ExitCode == 0, output.ToString(), error.ToString());
    }

    /// <summary>
    /// Parse winget upgrade list output
    /// Example output format:
    /// Name                          Id                                  Version      Available   Source
    /// -----------------------------------------------------------------------------------------
    /// Visual Studio Code            Microsoft.VisualStudioCode           1.75.0       1.76.0      winget
    /// </summary>
    private List<UpdateInfo> ParseUpgradeList(string output)
    {
        var updates = new List<UpdateInfo>();
        
        if (string.IsNullOrWhiteSpace(output))
            return updates;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        bool headerFound = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Find header line with dashes
            if (!headerFound && trimmed.Contains("---"))
            {
                headerFound = true;
                continue;
            }

            if (!headerFound)
                continue;

            // Skip empty or very short lines
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length < 20)
                continue;

            // Use regex to parse columns based on spacing
            var parts = Regex.Split(trimmed, @"\s{2,}")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToArray();

            if (parts.Length >= 4)
            {
                updates.Add(new UpdateInfo
                {
                    Name = parts[0],
                    Id = parts.Length > 1 ? parts[1] : "",
                    CurrentVersion = parts.Length > 2 ? parts[2] : "",
                    AvailableVersion = parts.Length > 3 ? parts[3] : "",
                    Source = parts.Length > 4 ? parts[4] : "winget"
                });
            }
        }

        return updates;
    }
}