using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CompleteUpload;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CompleteUpload;

public sealed class CompleteFileUploadHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CompleteFileUploadHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Complete upload handler should use storage metadata when creation scope is permitted")]
    [InlineData("Personal", false, "file-manager.personal-files.create.own")]
    [InlineData("Personal", true, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, "file-manager.avatars.create")]
    public async Task HandleAsync_Should_UseStorageMetadata_When_CreationScopeIsPermitted(
        string type, bool foreignOwner, string permission)
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, foreignOwner ? Guid.NewGuid() : _userId);
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).Returns(new FileMetadata("TEXT/PLAIN", 7));

        var result = await CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(7);
        _files.Received(1).Update(file);
        _files.DidNotReceiveWithAnyArgs().Delete(default!);
        await _storage.Received(1).GetMetadataAsync(file.StorageKey.Value, ct);
        await _storage.DidNotReceiveWithAnyArgs().DownloadAsync(default!, ct);
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default!, ct);
    }

    [Theory(DisplayName = "Complete upload handler should avoid updating metadata when upload is already complete")]
    [InlineData("Personal", "file-manager.personal-files.create.own")]
    [InlineData("Avatar", "file-manager.avatars.create")]
    public async Task HandleAsync_Should_AvoidUpdatingMetadata_When_UploadIsAlreadyComplete(
        string type, string permission)
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, _userId);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).Returns(new FileMetadata("text/plain", 4));

        var result = await CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Complete upload handler should hide the file when creation permission does not cover it")]
    [InlineData("Personal", false, "")]
    [InlineData("Personal", true, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, "file-manager.personal-files.read.all")]
    [InlineData("Personal", false, "file-manager.personal-files.update.own")]
    [InlineData("Avatar", false, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    public async Task HandleAsync_Should_HideFile_When_CreationPermissionDoesNotCoverIt(
        string type, bool foreignOwner, string permission)
    {
        _user.HasPermission(permission).Returns(true);
        var file = CreateFile(type, foreignOwner ? Guid.NewGuid() : _userId);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var result = await CreateHandler().HandleAsync(
            new CompleteFileUploadCommand(file.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        file.Status.ShouldBe(FileStatus.PendingUpload);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _storage.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Complete upload handler should reject lookup when identifier is invalid or missing")]
    [InlineData(false, ErrorType.NotFound)]
    [InlineData(true, ErrorType.Validation)]
    public async Task HandleAsync_Should_RejectLookup_When_IdentifierIsInvalidOrMissing(
        bool emptyId, ErrorType errorType)
    {
        var command = new CompleteFileUploadCommand(emptyId ? Guid.Empty : Guid.NewGuid());

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.Errors.ShouldContain(error => error.Type == errorType);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _storage.ReceivedCalls().ShouldBeEmpty();
        if (emptyId)
        {
            _files.ReceivedCalls().ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "Complete upload handler should preserve pending metadata when stored content is unusable")]
    [InlineData(null, 0)]
    [InlineData("text/*", 4)]
    [InlineData("text/plain", 0)]
    public async Task HandleAsync_Should_PreservePendingMetadata_When_StoredContentIsUnusable(
        string? contentType, long size)
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        FileMetadata? metadata = contentType is null ? null : new FileMetadata(contentType, size);
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).Returns(metadata);

        var result = await CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct);

        result.ShouldHaveSingleError(
            ErrorType.Conflict,
            contentType is null ? "File content has not been uploaded." : "Uploaded file metadata is invalid.");
        file.Status.ShouldBe(FileStatus.PendingUpload);
        file.ContentType.ShouldBeNull();
        file.Size.ShouldBeNull();
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _files.DidNotReceiveWithAnyArgs().Delete(default!);
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, ct);
    }

    [Fact(DisplayName = "Complete upload handler should preserve deletion intent when file is being deleted")]
    public async Task HandleAsync_Should_PreserveDeletionIntent_When_FileIsBeingDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        file.BeginDeletion();
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).Returns(new FileMetadata("text/plain", 4));

        var result = await CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Only pending uploads can be completed.");
        file.Status.ShouldBe(FileStatus.Deleting);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Complete upload handler should preserve confirmed metadata when storage metadata differs")]
    [InlineData("text/plain", 5)]
    [InlineData("application/octet-stream", 4)]
    public async Task HandleAsync_Should_PreserveConfirmedMetadata_When_StorageMetadataDiffers(
        string contentType, long size)
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).Returns(new FileMetadata(contentType, size));

        var result = await CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File content cannot be replaced after upload.");
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(4);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Complete upload handler should propagate storage failures when metadata lookup fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PropagateStorageFailures_When_MetadataLookupFails(bool cancelled)
    {
        var ct = TestContext.Current.CancellationToken;
        _user.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        _files.GetByIdAsync(file.Id, ct).Returns(file);
        Exception failure = cancelled ? new OperationCanceledException(ct) : new IOException("Storage unavailable");
        _storage.GetMetadataAsync(file.StorageKey.Value, ct).ThrowsAsync(failure);

        var thrown = await Record.ExceptionAsync(() =>
            CreateHandler().HandleAsync(new CompleteFileUploadCommand(file.Id.Value), ct));

        thrown.ShouldBeSameAs(failure);
        file.Status.ShouldBe(FileStatus.PendingUpload);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _files.DidNotReceiveWithAnyArgs().Delete(default!);
    }

    private static File CreateFile(string type, Guid ownerId)
    {
        var fileType = FileType.FromName(type);
        return File.Create(
            name: FileName.Create("file.txt").Value,
            type: fileType,
            userId: fileType == FileType.Personal ? UserId.Create(ownerId).Value : null).Value;
    }

    private CompleteFileUploadHandler CreateHandler()
    {
        return new CompleteFileUploadHandler(_files, _storage, new FileAccessPolicy(_user));
    }
}
