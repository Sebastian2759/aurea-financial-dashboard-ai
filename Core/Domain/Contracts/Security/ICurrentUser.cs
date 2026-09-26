namespace Domain.Contracts.Security;
public interface ICurrentUser
{
    string Id { get; }
    string Role { get; }
}
