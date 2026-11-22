using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetDetails;

public sealed class GetFileDetailsQueryValidatorTests
{
    [Theory(DisplayName = "Personal file details validator should validate the identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_IdValidityVaries(bool valid)
    {
        var query = new GetFileDetailsQuery(valid ? Guid.NewGuid() : Guid.Empty);

        var result = new GetFileDetailsQueryValidator().Validate(query);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(query.Id));
        }
    }
}
