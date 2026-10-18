using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units;

public sealed class UnitsConcurrencyEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Unit writes should return conflict when another transaction deletes the loaded unit")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task WriteUnit_Should_ReturnConflict_When_AnotherTransactionDeletesTheLoadedUnit(string method)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var api = new UnitsApiTestClient(Fixture);
        var faction = await api.CreateFactionAsync(cancellationToken);
        var unit = await api.CreateAsync(faction.Id, cancellationToken);
        var interceptor = new DeleteUnitBeforeSaveInterceptor(unit.Id, Fixture.Services);
        await using var testFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = testFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<IInterceptor>(interceptor)));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/v1/units/{unit.Id}");
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new UpdateUnitRequest(
                Name: "Swordsman",
                Description: "A disciplined melee unit.",
                Attack: 9,
                Defense: 10,
                Health: 20,
                MinimumDamage: 4,
                MaximumDamage: 6,
                Initiative: 9.5,
                Speed: 5,
                Morale: 3,
                Luck: 2,
                FactionId: faction.Id,
                Shots: null,
                RangedAttackRange: null));
        }

        using var response = await client.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        interceptor.UnitDeleted.ShouldBeTrue();
        interceptor.RequestHadTransaction.ShouldBeTrue();
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, responseBody);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var problem = JsonSerializer.Deserialize<JsonElement>(responseBody);
        problem.GetProperty("status").GetInt32().ShouldBe(409);
        problem.GetProperty("title").GetString().ShouldBe("Conflict");
        problem.GetProperty("detail").GetString()
            .ShouldBe("The entity was modified or deleted. Reload it and try again.");
        using var deletedUnitResponse = await client.GetAsync($"/api/v1/units/{unit.Id}", cancellationToken);
        deletedUnitResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var nextResponse = await client.PostAsJsonAsync(
            "/api/v1/units",
            UnitsApiTestClient.CreateRequest(faction.Id, "Next unit"),
            cancellationToken);
        nextResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    private sealed class DeleteUnitBeforeSaveInterceptor(Guid unitId, IServiceProvider services)
        : SaveChangesInterceptor
    {
        public bool UnitDeleted { get; private set; }
        public bool RequestHadTransaction { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken)
        {
            if (UnitDeleted
                || eventData.Context is not CompendiumWriteDbContext context
                || !context.ChangeTracker.Entries<Unit>().Any(entry => entry.Entity.Id.Value == unitId))
            {
                return result;
            }

            RequestHadTransaction = context.Database.CurrentTransaction is not null;
            await using var scope = services.CreateAsyncScope();
            var competingContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            var id = UnitId.Create(unitId).Value;
            UnitDeleted = await competingContext.Set<Unit>()
                .Where(unit => unit.Id == id)
                .ExecuteDeleteAsync(cancellationToken) == 1;

            return result;
        }
    }
}
