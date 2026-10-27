using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlHandlerTests
{
    [Theory(DisplayName = "Download URL handler should sign aggregate metadata when access is granted")]
    [InlineData("Personal", "file-manager.personal-files.read.own")]
    [InlineData("Avatar", "file-manager.avatars.read")]
    [InlineData("Personal", "file-manager.personal-files.read.all")]
    public async Task HandleAsync_Should_SignAggregateMetadata_When_AccessIsGranted(string typeName, string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(
            typeName,
            permission.EndsWith(".all", StringComparison.Ordinal) ? Guid.NewGuid() : userId);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var repository = Substitute.For<IFilesRepository>();
        repository.GetByIdAsync(file.Id, cancellationToken).Returns(file);
        var storage = Substitute.For<IFileStorage>();
        var signed = new PresignedDownloadUrl("https://storage.example/file?signature=original", DateTimeOffset.UtcNow);
        storage.GetPresignedDownloadUrlAsync(file.StorageKey.Value, file.Name.Value, "text/plain", cancellationToken)
            .Returns(signed);
        var handler = new GetFileDownloadUrlHandler(
            repository,
            storage,
            currentUser,
            new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFileDownloadUrlQuery(file.Id.Value), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(signed);
        result.Value.Url.ShouldBe(signed.Url);
        result.Value.ExpiresAt.ShouldBe(signed.ExpiresAt);
        await repository.Received(1).GetByIdAsync(file.Id, cancellationToken);
        await storage.Received(1).GetPresignedDownloadUrlAsync(
            file.StorageKey.Value, file.Name.Value, "text/plain", cancellationToken);
        storage.ReceivedCalls().Count().ShouldBe(1);
    }

    [Fact(DisplayName = "Download URL handler should reject the caller when user id is unavailable")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_UserIdIsUnavailable()
    {
        var currentUser = CreateCurrentUser(null);
        var repository = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetFileDownloadUrlHandler(
            repository,
            storage,
            currentUser,
            new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFileDownloadUrlQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Download URL handler should return not found when file is absent")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsAbsent()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetFileDownloadUrlHandler(
            Substitute.For<IFilesRepository>(), storage, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFileDownloadUrlQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Download URL handler should hide the file when access policy denies reading")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Avatar", "file-manager.personal-files.read.own")]
    public async Task HandleAsync_Should_ReturnNotFound_When_AccessPolicyDeniesReading(
        string typeName,
        string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(typeName, userId);
        var repository = Substitute.For<IFilesRepository>();
        repository.GetByIdAsync(file.Id, cancellationToken).Returns(file);
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetFileDownloadUrlHandler(
            repository,
            storage,
            currentUser,
            new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFileDownloadUrlQuery(file.Id.Value), cancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Download URL handler should reject signing when upload is not ready")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnConflict_When_UploadIsNotReady(bool deleting)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(true);
        var file = CreateFile("Personal", userId);
        if (deleting)
        {
            file.BeginDeletion();
        }

        var repository = Substitute.For<IFilesRepository>();
        repository.GetByIdAsync(file.Id, cancellationToken).Returns(file);
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetFileDownloadUrlHandler(
            repository,
            storage,
            currentUser,
            new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFileDownloadUrlQuery(file.Id.Value), cancellationToken);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File is not ready for download.");
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Download URL handler should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var repository = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        var handler = new GetFileDownloadUrlHandler(
            repository,
            storage,
            currentUser,
            new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFileDownloadUrlQuery(Guid.Empty), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        repository.ReceivedCalls().ShouldBeEmpty();
        storage.ReceivedCalls().ShouldBeEmpty();
    }

    private static ICurrentUser CreateCurrentUser(Guid? userId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        return currentUser;
    }

    private static File CreateFile(string typeName, Guid userId)
    {
        var type = FileType.FromName(typeName);
        return File.Create(
            FileName.Create("report.txt").Value,
            type,
            type == FileType.Personal ? UserId.Create(userId).Value : null).Value;
    }
}
