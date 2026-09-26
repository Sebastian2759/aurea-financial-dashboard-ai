using Domain.Contracts.Security;
namespace Api.Authentication;
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string Id => accessor.HttpContext?.User.FindFirst("sub")?.Value ?? "";
    public string Role => accessor.HttpContext?.User.FindFirst("role")?.Value ?? "";
}
