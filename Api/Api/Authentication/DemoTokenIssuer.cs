using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Contracts.Security;
using Domain.Security;
using Microsoft.IdentityModel.Tokens;
namespace Api.Authentication;
public sealed class DemoTokenIssuer(IConfiguration configuration, IHostEnvironment environment) : ISessionTokenIssuer
{
    public bool IsEnabled => configuration.GetValue<bool>("DemoAuth:Enabled") && (environment.IsDevelopment() || environment.IsEnvironment("Demo") || environment.IsEnvironment("Testing"));
    public SessionTicket Issue(DemoUser user)
    {
        if (!IsEnabled) throw new DemoDisabledException();
        var expires = DateTimeOffset.UtcNow.AddMinutes(60);
        var claims = new List<Claim> { new("sub", user.Id), new("name", user.Name), new("role", user.Role), new("jti", Guid.NewGuid().ToString()) };
        claims.AddRange(Permissions.ForRole(user.Role).Select(p => new Claim("permission", p)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var jwt = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, DateTime.UtcNow, expires.UtcDateTime, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
