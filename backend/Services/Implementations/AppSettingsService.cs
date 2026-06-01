using Investly.API.Data;
using Investly.API.Models.DTOs.AppSettings;
using Investly.API.Models.Entities;
using Investly.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Services.Implementations;

public class AppSettingsService : IAppSettingsService
{
    private readonly AppDbContext _db;

    public AppSettingsService(AppDbContext db) => _db = db;

    public async Task<AppSettingsResponse> GetAsync()
    {
        var settings = await GetOrCreateAsync();
        return Map(settings);
    }

    public async Task<AppSettingsResponse> UpdateAsync(UpdateAppSettingsRequest req)
    {
        var settings = await GetOrCreateAsync();

        if (req.MaintenanceMode.HasValue) settings.MaintenanceMode = req.MaintenanceMode.Value;
        if (req.MaintenanceMessageAr is not null) settings.MaintenanceMessageAr = req.MaintenanceMessageAr;
        if (req.MaintenanceMessageEn is not null) settings.MaintenanceMessageEn = req.MaintenanceMessageEn;
        if (req.AnnouncementActive.HasValue) settings.AnnouncementActive = req.AnnouncementActive.Value;
        if (req.AnnouncementAr is not null) settings.AnnouncementAr = req.AnnouncementAr;
        if (req.AnnouncementEn is not null) settings.AnnouncementEn = req.AnnouncementEn;
        if (req.AllowRegistration.HasValue) settings.AllowRegistration = req.AllowRegistration.Value;
        if (req.AllowInvestments.HasValue) settings.AllowInvestments = req.AllowInvestments.Value;
        if (req.MinSupportedVersion is not null) settings.MinSupportedVersion = req.MinSupportedVersion;
        settings.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Map(settings);
    }

    private async Task<AppSetting> GetOrCreateAsync()
    {
        var settings = await _db.AppSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new AppSetting { Id = 1 };
            _db.AppSettings.Add(settings);
            await _db.SaveChangesAsync();
        }
        return settings;
    }

    private static AppSettingsResponse Map(AppSetting s) => new(
        s.MaintenanceMode,
        s.MaintenanceMessageAr,
        s.MaintenanceMessageEn,
        s.AnnouncementActive,
        s.AnnouncementAr,
        s.AnnouncementEn,
        s.AllowRegistration,
        s.AllowInvestments,
        s.MinSupportedVersion,
        s.UpdatedAt
    );
}
