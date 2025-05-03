using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UsersRepository : IUsersRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<UsersRepository> _logger;

    public UsersRepository(IDbConnectionFactory dbConnectionFactory, ILogger<UsersRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(UsersRepository)} constructed");
    }

    public async Task<User?> GetUserByEmail(string userEmail)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserByEmail)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"SELECT id, first_name, last_name, email, username, authenticated_on, user_oid, 
                  is_email_verified, initialization_vector, 
                 terms_and_conditions_accepted_on, email_verification_code, is_enabled, meta, created_on, modified_on, 
                 deleted_on 
            FROM `modular_net`.`user` 
            WHERE email = @userEmail AND deleted_on IS NULL";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            userEmail
        });

        return user;
    }

    public async Task RegisterUser(User user)
    {
        _logger.LogDebug($"Start repository method {nameof(RegisterUser)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"INSERT INTO `modular_net`.`user` 
                (`id`, `first_name`, `last_name`, `email`, `username`, `authenticated_on`, `user_oid`, 
                 `is_email_verified`, `initialization_vector`, `is_subscriptions_password_set`, 
                 `terms_and_conditions_accepted_on`, `email_verification_code`, `is_enabled`, `meta`, `created_on`)
                VALUES 
                (@id, @first_name, @last_name, @email, @username, @authenticated_on, @user_oid, @is_email_verified, @initialization_vector, @is_subscriptions_password_set, 
                 @terms_and_conditions_accepted_on, @email_verification_code, @is_enabled, @meta, @created_on)
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = user.Id,
            first_name = user.FirstName,
            last_name = user.LastName,
            email = user.Email,
            username = user.Username,
            authenticated_on = user.AuthenticatedOn,
            user_oid = user.UserOid,
            is_email_verified = user.IsEmailVerified,
            initialization_vector = user.InitializationVector,
            terms_and_conditions_accepted_on = user.TermsAndConditionsAcceptedOn,
            email_verification_code = user.EmailVerificationCode,
            is_enabled = true, // Always set to true because the enabling is managed by Firebase at login time.
            meta = user.Meta,
            created_on = user.CreatedOn
        });
    }

    public async Task UpdateUserLoginDate(Guid userId, DateTime userAuthenticatedOn)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserLoginDate)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET authenticated_on = @authenticated_on, modified_on = @modified_on  
                WHERE id = @user_id 
             ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            authenticated_on = userAuthenticatedOn,
            user_id = userId,
            modified_on = DateTime.UtcNow
        });
    }

    public async Task<User?> GetUserById(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserById)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"SELECT id, first_name, last_name, email, username, authenticated_on, user_oid,
                 is_email_verified, initialization_vector, 
                 terms_and_conditions_accepted_on, email_verification_code, is_enabled, meta, created_on, modified_on, 
                 deleted_on 
            FROM `modular_net`.`user` 
            WHERE id = @id AND deleted_on IS NULL";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            id = userId
        });

        return user;
    }

    public async Task SetUserEmailAsVerified(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(SetUserEmailAsVerified)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `is_email_verified` = @is_email_verified, modified_on = @modified_on, is_enabled = @is_enabled
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_email_verified = true,
            is_enabled = true,
            modified_on = DateTime.UtcNow
        });
    }

    public async Task EnableUser(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(SetUserEmailAsVerified)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET is_enabled = @is_enabled, modified_on = @modified_on 
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_enabled = true,
            modified_on = DateTime.UtcNow
        });
    }

    public async Task UpdateUserOid(Guid userId, string? userOid)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserOid)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `user_oid` = @userOid, modified_on = @modified_on
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);

        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            userOid,
            modified_on = DateTime.UtcNow
        });
    }

    public async Task<Guid> GetUserIdByEmail(string emailFromToken)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserIdByEmail)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"SELECT id 
            FROM `modular_net`.`user` 
            WHERE email = @emailFromToken AND deleted_on IS NULL";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        var userId = await connection.QueryFirstOrDefaultAsync<Guid>(sql, new
        {
            emailFromToken
        });

        return userId;
    }

    public async Task SetTermsAndConditionsAsAccepted(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(SetTermsAndConditionsAsAccepted)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `terms_and_conditions_accepted_on` = @terms_and_conditions_accepted_on, modified_on = @modified_on, is_enabled = @is_enabled
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);

        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            terms_and_conditions_accepted_on = now,
            is_enabled = true,
            modified_on = now
        });
    }

    /// <summary>
    ///     To deactivate the account will set the flag Is Enabled to false.
    ///     Do not Soft Delete the user, otherwise the Get Email method won't return it and
    ///     after authenticated the user will try to be recreated
    /// </summary>
    /// <param name="userId"></param>
    public async Task DeactivateAccount(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(DeactivateAccount)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `is_enabled` = @is_enabled, modified_on = @modified_on
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);

        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_enabled = false,
            modified_on = now
        });
    }
}