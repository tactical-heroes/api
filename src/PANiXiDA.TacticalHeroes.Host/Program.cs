using JasperFx;

using PANiXiDA.Core.Presentation.Http.DependencyInjection;

using PANiXiDA.TacticalHeroes.Host.Common;
using PANiXiDA.TacticalHeroes.Host.Configurations;

using IdentityPresentationAssembly = PANiXiDA.TacticalHeroes.Identity.Presentation.PresentationAssembly;
using NotificationsPresentationAssembly = PANiXiDA.TacticalHeroes.Notifications.Presentation.PresentationAssembly;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = FilesConstants.FileRequestSizeLimit;
});

builder.Services.AddHttp(builder.Configuration);

builder.AddIdentityModule();
builder.AddNotificationsModule();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttp(
    IdentityPresentationAssembly.Instance,
    NotificationsPresentationAssembly.Instance);

return await app.RunJasperFxCommands(args);
