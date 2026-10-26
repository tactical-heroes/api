using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetPersonalDetails;

public sealed class GetPersonalFileDetailsQueryValidatorTests
{
    [Theory(DisplayName = "Personal file details validator should validate the identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_IdValidityVaries(bool valid)
    {
        var query = new GetPersonalFileDetailsQuery(valid ? Guid.NewGuid() : Guid.Empty);

        var result = new GetPersonalFileDetailsQueryValidator().Validate(query);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(query.Id));
        }
    }
}
