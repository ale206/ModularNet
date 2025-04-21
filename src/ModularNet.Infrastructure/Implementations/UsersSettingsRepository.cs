using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UsersSettingsRepository : IUsersSettingsRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<UsersSettingsRepository> _logger;

    public UsersSettingsRepository(IDbConnectionFactory dbConnectionFactory, ILogger<UsersSettingsRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(UsersSettingsRepository)} constructed");
    }

    public async Task<IEnumerable<UserSettingDto>> GetUserSettings(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserSettings)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"SELECT `id`, `user_id`, `setting_name`, `setting_value`, `is_enabled`, `meta`,
       `created_on`, `modified_on`, `deleted_on`
            FROM `user_setting` 
            WHERE user_id = @user_id AND is_enabled = 1 AND deleted_on IS NULL";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        var userSettingsDto = await connection.QueryAsync<UserSettingDto>(sql, new
        {
            user_id = userId
        });

        return userSettingsDto;
    }

    public async Task CreateUserSetting(UserSetting userSetting)
    {
        _logger.LogDebug($"Start repository method {nameof(CreateUserSetting)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"INSERT INTO `user_setting` 
                (`id`, `user_id`, `setting_name`, `setting_value`, `is_enabled`, `meta`, `created_on`)
                VALUES 
                (@id, @user_id, @setting_name, @setting_value, @is_enabled, @meta, @created_on)
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = userSetting.Id,
            user_id = userSetting.UserId,
            setting_name = userSetting.SettingName,
            setting_value = userSetting.SettingValue,
            is_enabled = true,
            meta = userSetting.Meta,
            created_on = DateTime.UtcNow
        });
    }

    public async Task UpdateUserSetting(UserSetting userSetting)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserSetting)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `user_setting` 
                SET setting_name = @setting_name, setting_value = @setting_value, is_enabled = @is_enabled, meta = @meta, modified_on = @modified_on  
                WHERE setting_name = @setting_name 
             ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            setting_name = userSetting.SettingName,
            setting_value = userSetting.SettingValue,
            is_enabled = userSetting.IsEnabled,
            meta = userSetting.Meta,
            modified_on = DateTime.UtcNow
        });
    }
}