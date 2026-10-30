using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UsersRepository : IUsersRepository
{
    private readonly ILogger<UsersRepository> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UsersRepository(IUnitOfWork unitOfWork, ILogger<UsersRepository> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        _logger.LogDebug($"{nameof(UsersRepository)} constructed");
    }

    public async Task<User?> GetUserByEmail(string userEmail)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserByEmail)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"SELECT id, first_name, last_name, email, username, authenticated_on, user_oid, 
                  is_email_verified, initialization_vector, 
                 terms_and_conditions_accepted_on, email_verification_code, is_enabled, meta, created_on, modified_on, 
                 deleted_on 
            FROM `modular_net`.`user` 
            WHERE email = @userEmail AND deleted_on IS NULL";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            userEmail
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task RegisterUser(User user)
    {
        _logger.LogDebug($"Start repository method {nameof(RegisterUser)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"INSERT INTO `modular_net`.`user` 
                (`id`, `first_name`, `last_name`, `email`, `username`, `authenticated_on`, `user_oid`, 
                 `is_email_verified`, `initialization_vector`, `is_subscriptions_password_set`, 
                 `terms_and_conditions_accepted_on`, `email_verification_code`, `is_enabled`, `meta`, `created_on`)
                VALUES 
                (@id, @first_name, @last_name, @email, @username, @authenticated_on, @user_oid, @is_email_verified, @initialization_vector, @is_subscriptions_password_set, 
                 @terms_and_conditions_accepted_on, @email_verification_code, @is_enabled, @meta, @created_on)
                ";

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
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateUserLoginDate(Guid userId, DateTime userAuthenticatedOn)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserLoginDate)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET authenticated_on = @authenticated_on, modified_on = @modified_on  
                WHERE id = @user_id 
             ";

        await connection.ExecuteAsync(sql, new
        {
            authenticated_on = userAuthenticatedOn,
            user_id = userId,
            modified_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task<User?> GetUserById(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserById)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"SELECT id, first_name, last_name, email, username, authenticated_on, user_oid,
                 is_email_verified, initialization_vector, 
                 terms_and_conditions_accepted_on, email_verification_code, is_enabled, meta, created_on, modified_on, 
                 deleted_on 
            FROM `modular_net`.`user` 
            WHERE id = @id AND deleted_on IS NULL";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            id = userId
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task SetUserEmailAsVerified(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(SetUserEmailAsVerified)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `is_email_verified` = @is_email_verified, modified_on = @modified_on, is_enabled = @is_enabled
                WHERE id = @id
                ";

        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_email_verified = true,
            is_enabled = true,
            modified_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task EnableUser(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(EnableUser)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET is_enabled = @is_enabled, modified_on = @modified_on 
                WHERE id = @id
                ";

        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_enabled = true,
            modified_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateUserOid(Guid userId, string? userOid)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateUserOid)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `user_oid` = @userOid, modified_on = @modified_on
                WHERE id = @id
                ";

        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            userOid,
            modified_on = DateTime.UtcNow
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task<Guid> GetUserIdByEmail(string emailFromToken)
    {
        _logger.LogDebug($"Start repository method {nameof(GetUserIdByEmail)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"SELECT id 
            FROM `modular_net`.`user` 
            WHERE email = @emailFromToken AND deleted_on IS NULL";

        return await connection.QueryFirstOrDefaultAsync<Guid>(sql, new
        {
            emailFromToken
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task SetTermsAndConditionsAsAccepted(Guid userId)
    {
        _logger.LogDebug($"Start repository method {nameof(SetTermsAndConditionsAsAccepted)}");

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `terms_and_conditions_accepted_on` = @terms_and_conditions_accepted_on, modified_on = @modified_on, is_enabled = @is_enabled
                WHERE id = @id
                ";

        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            terms_and_conditions_accepted_on = now,
            is_enabled = true,
            modified_on = now
        }, transaction: _unitOfWork.Transaction);
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

        var connection = await _unitOfWork.GetConnection();

        const string sql =
            @"UPDATE `modular_net`.`user` 
                SET `is_enabled` = @is_enabled, modified_on = @modified_on
                WHERE id = @id
                ";

        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, new
        {
            id = userId,
            is_enabled = false,
            modified_on = now
        }, transaction: _unitOfWork.Transaction);
    }
}
