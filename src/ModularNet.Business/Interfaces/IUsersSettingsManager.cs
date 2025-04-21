using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.Business.Interfaces;

public interface IUsersSettingsManager
{
    Task<List<UserSetting>> GetUserSettingsByUserId(Guid userId);

    Task<List<UserSetting>> CreateOrUpdateUserSettings(
        CreateOrUpdateUserSettingsRequest createOrUpdateUserSettingsRequest);
}