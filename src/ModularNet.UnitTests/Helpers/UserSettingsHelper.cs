using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.UnitTests.Helpers;

public static class UserSettingsHelper
{
    public static UserSetting GetUserSetting()
    {
        return new UserSetting
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SettingName = "theme",
            SettingValue = "dark",
            IsEnabled = true,
            Meta = null
        };
    }

    public static UserSettingDto GetUserSettingDto()
    {
        return new UserSettingDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SettingName = "theme",
            SettingValue = "dark",
            IsEnabled = true,
            CreatedOn = DateTime.UtcNow
        };
    }

    public static CreateOrUpdateUserSettingsRequest GetCreateOrUpdateUserSettingsRequest()
    {
        var userId = Guid.NewGuid();
        return new CreateOrUpdateUserSettingsRequest
        {
            UserSettings = new List<UserSetting>
            {
                new()
                {
                    UserId = userId,
                    SettingName = "theme",
                    SettingValue = "dark",
                    IsEnabled = true
                }
            }
        };
    }
}
