using System.Net.Mime;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.ApiExplorer;

using Scalar.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class ScalarConfiguration
{
    private const string ConfigurationPath = "/scalar/operation-order.js";

    internal static IServiceCollection AddScalarOrdering(this IServiceCollection services)
    {
        return services.Configure<ScalarOptions>(options => options.WithJavaScriptConfiguration(ConfigurationPath));
    }

    internal static WebApplication UseScalarOrdering(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapGet(ConfigurationPath, CreateConfiguration)
                .ExcludeFromDescription();
        }

        return app;
    }

    private static IResult CreateConfiguration(IApiDescriptionGroupCollectionProvider descriptions)
    {
        var creationPaths = descriptions.ApiDescriptionGroups.Items
            .SelectMany(group => group.Items)
            .Where(description => description.HttpMethod == HttpMethods.Post
                && description.RelativePath is not null
                && description.SupportedResponseTypes.Any(
                    response => response.StatusCode == StatusCodes.Status201Created))
            .Select(description => ("/" + description.RelativePath).TrimEnd('/'))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var configuration = $$"""
            const creationPaths = new Set({{JsonSerializer.Serialize(creationPaths)}});
            const methods = ['get', 'post', 'put', 'patch', 'delete'];

            function priority(operation) {
                const method = operation.method.toLowerCase();
                const index = methods.indexOf(method);
                const rank = (index < 0 ? methods.length : index) * 2;
                const path = operation.path.replace(/\/+$/, '');
                return rank + (method === 'post' && !creationPaths.has(path) ? 1 : 0);
            }

            export default {
                operationsSorter: (left, right) =>
                    priority(left) - priority(right) || left.path.localeCompare(right.path),
            };
            """;

        return Results.Text(configuration, MediaTypeNames.Text.JavaScript);
    }
}
