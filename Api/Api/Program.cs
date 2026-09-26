using System.Text;
using Adapters;
using Api.Authentication;
using Api.Handlers;
using Api.Realtime;
using Api.Diagnostics;
using Domain.Contracts.Diagnostics;
using Application;
using Asp.Versioning.Conventions;
using Domain.Contracts.Security;
using Domain.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Context;
using Scalar.AspNetCore;
using ApiKeyOptions = Api.Authentication.ApiKeyOptions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new Api.Serialization.UtcDateTimeConverter()));
builder.Services.AddOpenApi();
builder.Services.AddApiVersioning(o => o.ReportApiVersions = true)
    .AddMvc(o => o.Conventions.Add(new VersionByNamespaceConvention()))
    .AddApiExplorer(o => { o.GroupNameFormat = "'v'VVV"; o.SubstituteApiVersionInUrl = true; });
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"]).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
var secret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32) throw new InvalidOperationException("Configura Jwt:Secret (mínimo 32 caracteres) fuera del repositorio.");
var apiKey = builder.Configuration["ApiKey:Key"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 16) throw new InvalidOperationException("Configura ApiKey:Key (mínimo 16 caracteres) fuera del repositorio.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new() { ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"], ValidateLifetime = true, ClockSkew = TimeSpan.Zero, NameClaimType = "name", RoleClaimType = "role", ValidAlgorithms = [SecurityAlgorithms.HmacSha256] };
        o.Events = new JwtBearerEvents { OnMessageReceived = context =>
        {
            if (context.Request.Path.StartsWithSegments("/hubs/market") && context.Request.Query.TryGetValue("access_token", out var token)) context.Token = token;
            return Task.CompletedTask;
        }};
    })
    .AddScheme<ApiKeyOptions, ApiKeyAuthHandler>(ApiKeyDefaults.SchemeName, o => { o.HeaderName = "X-Api-Key"; o.Key = apiKey; });
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
    foreach (var permission in Permissions.All)
        o.AddPolicy(permission, p => p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().RequireClaim("permission", permission));
    o.AddPolicy("JwtOnly", p => p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser());
    o.AddPolicy("ApiKeyOnly", p => p.AddAuthenticationSchemes(ApiKeyDefaults.SchemeName).RequireAuthenticatedUser());
    o.AddPolicy("BearerOrApiKey", p => p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyDefaults.SchemeName).RequireAuthenticatedUser());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ISessionTokenIssuer, DemoTokenIssuer>();
builder.Services.AddAdapterServices();
builder.Services.AddMarketDataServices(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddSingleton<MarketSubscriptions>();
builder.Services.AddHostedService<MarketBroadcastWorker>();
builder.Services.AddSingleton<SystemEventBuffer>();
builder.Services.AddSingleton<ISystemEventSink>(services => services.GetRequiredService<SystemEventBuffer>());
builder.Services.AddHostedService<SystemEventWriter>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Demo"))
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapGet("/health/ready", async (AppDbContext db, CancellationToken ct) => await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503)).AllowAnonymous();
app.MapControllers();
app.MapHub<MarketHub>("/hubs/market", o => o.CloseOnAuthenticationExpiration = true);
if (builder.Configuration.GetValue("Database:Initialize", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch (Exception) when (attempt < 12) { await Task.Delay(TimeSpan.FromSeconds(5)); }
    }
}
app.Run();
public partial class Program { }
