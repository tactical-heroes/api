using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal static class InfrastructureServiceCollectionFactory
{
    internal static IServiceCollection Create(Assembly infrastructureAssembly)
    {
        var addInfrastructureMethod = infrastructureAssembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly))
            .Single(method =>
            {
                var parameters = method.GetParameters();

                return string.Equals(
                    method.Name,
                    "AddInfrastructure",
                    StringComparison.Ordinal)
                    && parameters.Length >= 2
                    && parameters[0].ParameterType
                    == typeof(IServiceCollection)
                    && parameters[1].ParameterType
                    == typeof(IConfiguration);
            });
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnectionString"] =
                    "Host=localhost;Database=architecture-tests",
                ["FileManager:S3Storage:BucketName"] = "architecture-tests"
            })
            .Build();
        var arguments = addInfrastructureMethod
            .GetParameters()
            .Select(parameter =>
            {
                if (parameter.ParameterType == typeof(IServiceCollection))
                {
                    serviceCollection.TryAddScoped<ICurrentUser>(_ => new AnonymousCurrentUser());

                    return serviceCollection;
                }

                if (parameter.ParameterType == typeof(IConfiguration))
                {
                    return configuration;
                }

                return parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : null;
            })
            .ToArray();

        addInfrastructureMethod.Invoke(
            obj: null,
            parameters: arguments);

        serviceCollection.TryAddScoped<ICurrentUser>(_ => new AnonymousCurrentUser());

        return serviceCollection;
    }

    private sealed class AnonymousCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => false;
        public Guid? UserId => null;
        public string? UserName => null;
        public string? AccessToken => null;
        public IReadOnlyCollection<string> Roles => [];

        public bool TryGetClaimValue<T>(string claimType, [MaybeNullWhen(false)] out T value)
            where T : IParsable<T>
        {
            value = default;
            return false;
        }

        public bool HasPermission(string permission)
        {
            return false;
        }
    }
}
