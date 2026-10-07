using JasperFx;

using PANiXiDA.TacticalHeroes.Host.Common;
using PANiXiDA.TacticalHeroes.Host.Configurations;
using PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

var builder = WebApplication.CreateBuilder(args);

var isObservabilityEnabled = !builder.Environment.IsEnvironment(EnvironmentConstants.Test)
    && !args.Contains("codegen", StringComparer.OrdinalIgnoreCase);

if (isObservabilityEnabled)
{
    builder.AddObservability();
}

builder.AddHttp();
builder.AddIdentityModule();
builder.AddNotificationsModule();
builder.AddCompendiumModule();
builder.AddFileManagerModule();
builder.AddMessaging();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttp();

return await app.RunJasperFxCommands(args);
