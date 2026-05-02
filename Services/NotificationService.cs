using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using WingetTrayUpdater.Models;

namespace WingetTrayUpdater.Services;

public class NotificationService
{
    private LocalizationService _loc => LocalizationService.Instance;

    public void ShowUpdatesAvailable(List<UpdateInfo> updates)
    {
        if (updates.Count == 0) return;

        try
        {
            var updateCount = updates.Count;
            var firstFew = updates.Take(3).Select(u => u.Name).ToArray();
            var title = string.Format(_loc["UpdatesNotificationTitle"], updateCount);

            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(string.Join("\n", firstFew));
            
            if (updateCount > 3)
            {
                builder.AddText("and " + (updateCount - 3) + " more...");
            }

            builder
                .AddButton(new ToastButton()
                    .SetContent(_loc["UpdateAllButton"])
                    .AddArgument("action", "updateAll"))
                .AddButton(new ToastButton()
                    .SetContent(_loc["DetailsButton"])
                    .AddArgument("action", "showDetails"));

            builder.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Failed to show notification: " + ex.Message);
        }
    }

    public void ShowUpdateComplete(int count, bool success)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(success ? _loc["UpdateCompleteTitle"] : _loc["UpdateErrorTitle"])
                .AddText(success 
                    ? string.Format(_loc["UpdateSuccessMsg"], count) 
                    : "Check log for details");

            builder.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Failed to show notification: " + ex.Message);
        }
    }

    public void ShowNotification(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Failed to show notification: " + ex.Message);
        }
    }

    public void ShowError(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("ERROR: " + title)
                .AddText(message)
                .Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Failed to show notification: " + ex.Message);
        }
    }
}