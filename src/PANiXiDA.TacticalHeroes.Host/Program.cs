using JasperFx;

using PANiXiDA.Core.Presentation.Http.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Configurations;
using PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();
builder.AddHttp();
builder.AddIdentityModule();
builder.AddNotificationsModule();
builder.AddCompendiumModule();
builder.AddMessaging();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttp();

return await app.RunJasperFxCommands(args);
