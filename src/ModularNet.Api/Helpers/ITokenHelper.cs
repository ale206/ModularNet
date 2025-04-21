namespace ModularNet.Api.Helpers;

public interface ITokenHelper
{
    Task<string?> GetEmailFromToken(HttpContext context);
}