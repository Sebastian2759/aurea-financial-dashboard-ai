using Domain.Contracts.Security;
using Domain.Security;
namespace Application.Security;
public static class AccessRules
{
    public static void Require(ICurrentUser user, string permission)
    {
        if (string.IsNullOrEmpty(user.Id) || !Permissions.ForRole(user.Role).Contains(permission)) throw new AccessDeniedException();
    }
    public static void RequireOwner(ICurrentUser user, string ownerId, string permission)
    {
        Require(user, permission);
        if (user.Id != ownerId && !Permissions.ForRole(user.Role).Contains(Permissions.WatchlistManageAll)) throw new AccessDeniedException();
    }
}
