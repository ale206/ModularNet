using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface IUsersSettingsRepository
{
    Task<IEnumerable<UserSettingDto>> GetUserSettings(Guid userId);
    Task CreateUserSetting(UserSetting userSetting);
    Task UpdateUserSetting(UserSetting userSetting);
}