using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UsersSettingsRepository : IUsersSettingsRepository
{
    private readonly ILogger<UsersSettingsRepository> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UsersSettingsRepository(IUnitOfWork unitOfWork, ILogger<UsersSettingsRepository> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        _logger.LogDebug($"{nameof(UsersSettingsRepository)} constructed");
    }

    public async Task<IEnumerable<UserSettingDto>> GetUserSettings(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserSettings)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"SELECT `id`, `user_id`, `setting_name`, `setting_value`, `is_enabled`, `meta`,
       `created_on`, `modified_on`, `deleted_on`
            FROM `user_setting` 
            WHERE user_id = @user_id AND is_enabled = 1 AND deleted_on IS NULL";

        return await connection.QueryAsync<UserSettingDto>(sql, new
        {
            user_id = userId
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task CreateUserSetting(UserSetting userSetting)
    {
        _logger.LogDebug($"Start repository method {nameof(CreateUserSetting)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"INSERT INTO `user_setting` 
                (`id`, `user_id`, `setting_name`, `setting_value`, `is_enabled`, `meta`, `created_on`)
                VALUES 
                (@id, @user_id, @setting_name, @setting_value, @is_enabled, @meta, @created_on)
                ";

        await connection.ExecuteAsync(sql, new
        {
            id = userSetting.Id,
            user_id = userSetting.UserId,
            setting_name = userSetting.SettingName,
            setting_value = userSetting.SettingValue,
            is_enabled = true,
            meta = userSetting.Meta,
            created_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateUserSetting(UserSetting userSetting)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserSetting)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `user_setting` 
                SET setting_name = @setting_name, setting_value = @setting_value, is_enabled = @is_enabled, meta = @meta, modified_on = @modified_on  
                WHERE setting_name = @setting_name 
             ";

        await connection.ExecuteAsync(sql, new
        {
            setting_name = userSetting.SettingName,
            setting_value = userSetting.SettingValue,
            is_enabled = userSetting.IsEnabled,
            meta = userSetting.Meta,
            modified_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }
}
