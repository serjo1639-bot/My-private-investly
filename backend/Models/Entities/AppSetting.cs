namespace Investly.API.Models.Entities;

/// <summary>
/// Singleton row (Id = 1) holding admin-controlled runtime settings that the
/// mobile app reads on launch — a lightweight remote-control / kill-switch.
/// </summary>
public class AppSetting
{
    public int Id { get; set; } = 1;

    // Maintenance mode — when on, the mobile app shows a blocking screen.
    public bool MaintenanceMode { get; set; }
    public string MaintenanceMessageAr { get; set; } = "التطبيق قيد الصيانة حالياً. يرجى المحاولة لاحقاً.";
    public string MaintenanceMessageEn { get; set; } = "The app is under maintenance. Please try again later.";

    // Announcement banner — shown at the top of the mobile home screen when active.
    public bool AnnouncementActive { get; set; }
    public string AnnouncementAr { get; set; } = string.Empty;
    public string AnnouncementEn { get; set; } = string.Empty;

    // Feature flags the app can honour.
    public bool AllowRegistration { get; set; } = true;
    public bool AllowInvestments { get; set; } = true;

    // Soft "please update" prompt (non-blocking) keyed on a minimum version string.
    public string MinSupportedVersion { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
