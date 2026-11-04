namespace ModularNet.Business.Interfaces;

public interface IUserNameManager
{
    Task<string?> GetUserFirstNameById(Guid userId);
}
