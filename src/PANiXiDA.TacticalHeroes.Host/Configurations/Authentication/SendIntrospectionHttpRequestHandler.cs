using OpenIddict.Validation;

using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Authentication;

internal sealed class SendIntrospectionHttpRequestHandler : IOpenIddictValidationHandler<ApplyIntrospectionRequestContext>
{
    public async ValueTask HandleAsync(ApplyIntrospectionRequestContext context)
    {
        var request = context.Transaction.GetHttpRequestMessage()
            ?? throw new InvalidOperationException("The introspection HTTP request was not initialized.");
        using var client = context.Transaction.GetHttpClient()
            ?? throw new InvalidOperationException("The introspection HTTP client was not initialized.");

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, context.CancellationToken);

        context.Transaction.SetProperty(typeof(HttpResponseMessage).FullName!, response);
    }
}
