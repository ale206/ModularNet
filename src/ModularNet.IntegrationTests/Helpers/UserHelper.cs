using ModularNet.Domain.Entities;

namespace ModularNet.IntegrationTests.Helpers;

public static class UserHelper
{
    public static User GetUser()
    {
        return new User
        {
            Email = "your.email@gmail.com",
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName"
        };
    }
}