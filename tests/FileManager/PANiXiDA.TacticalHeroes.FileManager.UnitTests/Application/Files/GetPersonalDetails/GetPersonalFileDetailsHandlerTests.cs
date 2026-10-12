using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetPersonalDetails;

public sealed class GetPersonalFileDetailsHandlerTests
{
    [Theory(DisplayName = "Personal file details handler should use the current owner when file availability varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_QueryCurrentOwner_When_FileAvailabilityVaries(bool found)
    {
        var userId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var file = new PersonalFileDetailsReadModel(
            Guid.NewGuid(), "file.txt", null, "Ready", "text/plain", 1024);
        var repository = Substitute.For<IFilesReadRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;
        repository.GetPersonalDetailsByIdAsync(file.Id, userId, cancellationToken)
            .Returns(found ? file : null);
        var handler = new GetPersonalFileDetailsHandler(repository, currentUser);

        var result = await handler.HandleAsync(new GetPersonalFileDetailsQuery(file.Id), cancellationToken);

        if (found)
        {
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBe(file);
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        }

        await repository.Received(1).GetPersonalDetailsByIdAsync(file.Id, userId, cancellationToken);
    }

    [Theory(DisplayName = "Personal file details handler should reject the caller when user id is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnUnauthorizedWithoutReading_When_UserIdIsUnavailable(bool emptyUserId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(emptyUserId ? Guid.Empty : null);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetPersonalFileDetailsHandler(repository, currentUser);

        var result = await handler.HandleAsync(
            new GetPersonalFileDetailsQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }
}
