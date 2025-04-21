using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.Business.Interfaces;

public interface IUsersManager
{
    Task<Guid> RegisterUserInModularNet(RegisterUserRequest user);
    Task LoginUser(UserSignInRequest userSignInRequest);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(Guid userId);
    Task SetUserEmailAsVerified(Guid userId);
    Task SetTermsAndConditionsAsAccepted(Guid userId);
    Task DeactivateAccount(Guid userId);
    Task EnableUser(Guid userId);
    Task<string?> RegisterUserInFirebase(string email, string password);
    Task<string?> LoginUserInFirebase(string email, string password);
    Task UpdateUserOid(Guid userId, string? userOid);
    Task<Guid> GetUserIdByEmail(string emailFromToken);
}