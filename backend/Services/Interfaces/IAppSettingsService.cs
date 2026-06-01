using Investly.API.Models.DTOs.AppSettings;

namespace Investly.API.Services.Interfaces;

public interface IAppSettingsService
{
    Task<AppSettingsResponse> GetAsync();
    Task<AppSettingsResponse> UpdateAsync(UpdateAppSettingsRequest request);
}
