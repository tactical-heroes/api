using JasperFx;

using PANiXiDA.Core.Presentation.Scalar.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Configurations;
using PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();
builder.AddHttp();
builder.AddSignalR();
builder.Services.AddApiDocumentation(builder.Configuration);
builder.AddIdentityModule();
builder.AddNotificationsModule();
builder.AddCompendiumModule();
builder.AddBattleModule();
builder.AddMatchmakingModule();
builder.AddGamesModule();
builder.AddBuildsModule();
builder.AddPlayersModule();
builder.AddFileManagerModule();
builder.AddMessaging();

var app = builder.Build();

app.UseSignalR();
app.UseHttp();
app.UseApiDocumentation();

return await app.RunJasperFxCommands(args);
