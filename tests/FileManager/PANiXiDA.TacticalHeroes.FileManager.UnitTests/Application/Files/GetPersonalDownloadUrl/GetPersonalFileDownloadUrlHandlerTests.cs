using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetPersonalDownloadUrl;

public sealed class GetPersonalFileDownloadUrlHandlerTests
{
    [Fact(DisplayName = "Download URL handler should sign persisted metadata when the owner requests a ready file")]
    public async Task HandleAsync_Should_SignPersistedMetadata_When_TheOwnerRequestsAReadyFile()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var file = new PersonalFileDownloadReadModel(
            Guid.NewGuid(), "report.txt", "legacy/persisted-key", "Ready", "text/plain");
        var repository = Substitute.For<IFilesReadRepository>();
        repository.GetPersonalDownloadByIdAsync(file.Id, userId, cancellationToken).Returns(file);
        var storage = Substitute.For<IFileStorage>();
        var signed = new PresignedDownloadUrl("https://storage.example/file?signature=original", DateTimeOffset.UtcNow);
        storage.GetPresignedDownloadUrlAsync(file.StorageKey, file.Name, file.ContentType!, cancellationToken)
            .Returns(signed);
        var handler = new GetPersonalFileDownloadUrlHandler(repository, storage, currentUser);

        var result = await handler.HandleAsync(new GetPersonalFileDownloadUrlQuery(file.Id), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Url.ShouldBe(signed.Url);
        result.Value.ExpiresAt.ShouldBe(signed.ExpiresAt);
        await repository.Received(1).GetPersonalDownloadByIdAsync(file.Id, userId, cancellationToken);
        await storage.Received(1).GetPresignedDownloadUrlAsync(
            file.StorageKey, file.Name, file.ContentType!, cancellationToken);
        storage.ReceivedCalls().Count().ShouldBe(1);
    }

    [Theory(DisplayName = "Download URL handler should reject the caller when user id is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_UserIdIsUnavailable(bool emptyUserId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(emptyUserId ? Guid.Empty : null);
        var repository = Substitute.For<IFilesReadRepository>();
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetPersonalFileDownloadUrlHandler(repository, storage, currentUser);

        var result = await handler.HandleAsync(
            new GetPersonalFileDownloadUrlQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Download URL handler should return not found when file is absent")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsAbsent()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetPersonalFileDownloadUrlHandler(
            Substitute.For<IFilesReadRepository>(), storage, currentUser);

        var result = await handler.HandleAsync(
            new GetPersonalFileDownloadUrlQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Download URL handler should reject signing when file is not downloadable")]
    [InlineData("PendingUpload", null)]
    [InlineData("Deleting", "text/plain")]
    [InlineData("Ready", null)]
    [InlineData("Ready", " ")]
    public async Task HandleAsync_Should_ReturnConflict_When_FileIsNotDownloadable(string status, string? contentType)
    {
        var userId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var file = new PersonalFileDownloadReadModel(Guid.NewGuid(), "file.txt", "key", status, contentType);
        var repository = Substitute.For<IFilesReadRepository>();
        repository.GetPersonalDownloadByIdAsync(file.Id, userId, TestContext.Current.CancellationToken).Returns(file);
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetPersonalFileDownloadUrlHandler(repository, storage, currentUser);

        var result = await handler.HandleAsync(
            new GetPersonalFileDownloadUrlQuery(file.Id), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File is not ready for download.");
        storage.ReceivedCalls().ShouldBeEmpty();
    }
}
