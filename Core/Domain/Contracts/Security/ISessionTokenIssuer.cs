using Domain.Security;
namespace Domain.Contracts.Security;
public sealed record SessionTicket(string AccessToken, DateTimeOffset ExpiresAt);
public interface ISessionTokenIssuer
{
    bool IsEnabled { get; }
    SessionTicket Issue(DemoUser user);
}
