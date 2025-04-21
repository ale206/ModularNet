using ModularNet.Api.Helpers;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;

public class UserIdExtractionMiddleware
{
    private readonly RequestDelegate _next;

    public UserIdExtractionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUsersManager usersManager, ITokenHelper tokenHelper,
        ICacheManager cacheManager)
    {
        try
        {
            var emailFromToken = await tokenHelper.GetEmailFromToken(context);

            if (!string.IsNullOrEmpty(emailFromToken))
            {
                var userIdKey = $"{emailFromToken}-userId";
                var userIdFromCache = await cacheManager.GetFromCache<string>(userIdKey, CacheType.InMemory);

                if (!string.IsNullOrEmpty(userIdFromCache))
                {
                    context.Items["UserId"] = userIdFromCache;
                }
                else
                {
                    var userId = await usersManager.GetUserIdByEmail(emailFromToken);
                    context.Items["UserId"] = userId;
                    await cacheManager.SaveInCache(userIdKey, userId.ToString(), CacheType.InMemory);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        await _next(context);
    }
}