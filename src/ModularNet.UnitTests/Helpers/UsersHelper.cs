using ModularNet.Domain.Entities;

namespace ModularNet.UnitTests.Helpers;

public static class UsersHelper
{
    public static User GetUser()
    {
        return new User
        {
            Email = "your.email@email.com",
            FirstName = "FirstName",
            LastName = "LastName",
            Id = Guid.NewGuid(),
            IsEmailVerified = true,
            CreatedOn = DateTime.UtcNow,
            AuthenticatedOn = DateTime.UtcNow,
            UserOid = "userOid",
            Username = "username"
        };
    }
}