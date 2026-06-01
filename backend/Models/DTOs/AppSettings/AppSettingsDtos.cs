namespace Investly.API.Models.DTOs.AppSettings;

public record AppSettingsResponse(
    bool MaintenanceMode,
    string MaintenanceMessageAr,
    string MaintenanceMessageEn,
    bool AnnouncementActive,
    string AnnouncementAr,
    string AnnouncementEn,
    bool AllowRegistration,
    bool AllowInvestments,
    string MinSupportedVersion,
    DateTime UpdatedAt
);

/// <summary>All fields optional — only provided values are applied (partial update).</summary>
public record UpdateAppSettingsRequest(
    bool? MaintenanceMode,
    string? MaintenanceMessageAr,
    string? MaintenanceMessageEn,
    bool? AnnouncementActive,
    string? AnnouncementAr,
    string? AnnouncementEn,
    bool? AllowRegistration,
    bool? AllowInvestments,
    string? MinSupportedVersion
);
