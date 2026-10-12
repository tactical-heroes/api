using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims.GetSections;

public sealed class GetSectionsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return the shared sections when query is handled")]
    public async Task HandleAsync_Should_ReturnSharedSections_When_QueryIsHandled()
    {
        var handler = new GetSectionsHandler();

        var result = await handler.HandleAsync(new GetSectionsQuery(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(SectionCatalog.All.Count);
        foreach (var section in SectionCatalog.All)
        {
            var item = result.Value.Single(item => item.Code == section.Code);
            item.DisplayName.ShouldBe(section.DisplayName);
        }
    }
}
