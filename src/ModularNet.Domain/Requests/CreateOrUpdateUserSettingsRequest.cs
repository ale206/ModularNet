using ModularNet.Domain.Entities;

namespace ModularNet.Domain.Requests;

public class CreateOrUpdateUserSettingsRequest
{
    public List<UserSetting> UserSettings { get; set; } = new();
}