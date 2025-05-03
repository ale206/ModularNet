using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetUserByEmail(string userEmail);
    Task RegisterUser(User user);
    Task UpdateUserLoginDate(Guid userId, DateTime userAuthenticatedOn);
    Task<User?> GetUserById(Guid userId);
    Task SetUserEmailAsVerified(Guid userId);
    Task SetTermsAndConditionsAsAccepted(Guid userId);
    Task DeactivateAccount(Guid userId);
    Task EnableUser(Guid userId);
    Task UpdateUserOid(Guid userId, string? userOid);
    Task<Guid> GetUserIdByEmail(string emailFromToken);
}