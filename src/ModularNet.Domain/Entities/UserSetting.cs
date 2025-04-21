namespace ModularNet.Domain.Entities;

public class UserSetting
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public bool IsEnabled { get; set; }
}