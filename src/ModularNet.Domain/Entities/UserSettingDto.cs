namespace ModularNet.Domain.Entities;

public class UserSettingDto : BaseEntity
{
    public Guid UserId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}