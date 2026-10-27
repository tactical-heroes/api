using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetPersonalDownloadUrl;

public sealed class GetPersonalFileDownloadUrlQueryValidatorTests
{
    [Theory(DisplayName = "Download URL validator should validate the identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_IdValidityVaries(bool valid)
    {
        var query = new GetPersonalFileDownloadUrlQuery(valid ? Guid.NewGuid() : Guid.Empty);

        var result = new GetPersonalFileDownloadUrlQueryValidator().Validate(query);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(query.Id));
        }
    }
}
