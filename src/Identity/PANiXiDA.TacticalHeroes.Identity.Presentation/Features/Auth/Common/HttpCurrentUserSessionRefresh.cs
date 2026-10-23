using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal sealed class HttpCurrentUserSessionRefresh(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserSessionRefresh
{
    internal const string HeaderName = "X-Session-Refresh-Required";

    private static readonly object s_refreshRequiredKey = new();

    public void RequireRefresh()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null || context.Items.ContainsKey(s_refreshRequiredKey))
        {
            return;
        }

        context.Items[s_refreshRequiredKey] = true;
        context.Response.OnStarting(
            static state =>
            {
                var response = (HttpResponse)state;
                if (response.StatusCode is >= 200 and < 300)
                {
                    response.Headers[HeaderName] = "true";
                }

                return Task.CompletedTask;
            },
            context.Response);
    }
}
