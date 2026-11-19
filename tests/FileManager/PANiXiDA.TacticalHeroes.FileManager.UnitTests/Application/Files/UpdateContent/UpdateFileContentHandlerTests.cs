using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.UpdateContent;

public sealed class UpdateFileContentHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();

    public UpdateFileContentHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.HasPermission("file-manager.avatars.update").Returns(true);
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(call.ArgAt<CancellationToken>(1)));
    }

    [Fact(DisplayName = "Content update should reserve its operation before uploading the same key")]
    public async Task HandleAsync_Should_ReserveOperation_When_UploadStarts()
    {
        var file = CreateFile();
        var key = file.StorageKey;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.UploadAsync(
                key.Value, Arg.Any<Stream>(), "text/plain",
                Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                file.Status.ShouldBe(FileStatus.Updating);
                file.VersionId!.Value.ShouldBe("initial");
                call.Arg<IReadOnlyDictionary<string, string>>()[FileMetadataNames.ContentUpdateId]
                    .ShouldBe(file.ContentUpdateId!.Value.ToString("D"));
                _unitOfWork.ReceivedCalls().Count().ShouldBe(1);
                return new FileUploadResult("replacement");
            });
        _storage.GetMetadataAsync(key.Value, "replacement", Arg.Any<CancellationToken>())
            .Returns(_ => UpdatedMetadata(file));
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await CreateHandler().HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.StorageKey.ShouldBe(key);
        file.VersionId!.Value.ShouldBe("replacement");
        file.ContentUpdateId.ShouldBeNull();
        file.Status.ShouldBe(FileStatus.Ready);
        file.Size!.Value.ShouldBe(3);
        await _unitOfWork.Received(2).SaveChangesAsync(TestContext.Current.CancellationToken);
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should retain active content when upload cannot be confirmed")]
    [InlineData("upload-error")]
    [InlineData("metadata-error")]
    [InlineData("save-error")]
    [InlineData("missing")]
    [InlineData("content-type")]
    [InlineData("size")]
    [InlineData("operation")]
    [InlineData("version")]
    [InlineData("versionless")]
    public async Task HandleAsync_Should_RetainActiveContent_When_UploadCannotBeConfirmed(string condition)
    {
        var file = CreateFile();
        var key = file.StorageKey;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.UploadAsync(
                Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new FileUploadResult(condition == "versionless" ? null : "replacement"));
        _storage.GetMetadataAsync(key.Value, "replacement", Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (condition == "metadata-error")
            {
                throw new IOException("Metadata failed.");
            }

            return condition == "missing" ? null : UpdatedMetadata(file) with
            {
                ContentType = condition == "content-type" ? "image/png" : "text/plain",
                Size = condition == "size" ? 99 : 3,
                VersionId = condition == "version" ? "different" : "replacement",
                Metadata = new Dictionary<string, string>
                {
                    [FileMetadataNames.ContentUpdateId] =
                        (condition == "operation" ? Guid.NewGuid() : file.ContentUpdateId!.Value).ToString("D")
                }
            };
        });
        if (condition == "upload-error")
        {
            _storage.UploadAsync(
                    Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                    Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new IOException("Upload failed."));
        }
        if (condition == "save-error")
        {
            var saves = 0;
            _unitOfWork.When(work => work.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(_ =>
            {
                if (++saves == 2)
                {
                    throw new IOException("Database failed.");
                }
            });
        }
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await CreateHandler().HandleAsync(
                new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content),
                TestContext.Current.CancellationToken);
            result.IsFailure.ShouldBeTrue();
        });

        if (condition.EndsWith("error", StringComparison.Ordinal))
        {
            exception.ShouldBeOfType<IOException>();
        }
        else
        {
            exception.ShouldBeNull();
        }
        file.StorageKey.ShouldBeSameAs(key);
        if (condition != "save-error")
        {
            file.Status.ShouldBe(FileStatus.Updating);
            file.ContentUpdateId.ShouldNotBeNull();
            file.VersionId!.Value.ShouldBe("initial");
            file.Size!.Value.ShouldBe(4);
        }
        await _unitOfWork.Received(condition == "save-error" ? 2 : 1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should bind the stored version when a legacy file has no active version")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_Should_BindStoredVersion_When_LegacyFileHasNoActiveVersion(bool versioned)
    {
        var file = CreateFile(ready: false);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.GetMetadataAsync(file.StorageKey.Value, Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 4,
                System.Collections.ObjectModel.ReadOnlyDictionary<string, string>.Empty,
                versioned ? "initial" : null));
        _storage.UploadAsync(
                Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new FileUploadResult("replacement"));
        _storage.GetMetadataAsync(file.StorageKey.Value, "replacement", Arg.Any<CancellationToken>())
            .Returns(_ => UpdatedMetadata(file));
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await CreateHandler().HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBe(versioned);
        file.VersionId?.Value.ShouldBe(versioned ? "replacement" : null);
        await _unitOfWork.Received(versioned ? 2 : 0).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should avoid storage when the request is invalid or unavailable")]
    [InlineData("id")]
    [InlineData("content-type")]
    [InlineData("size")]
    [InlineData("stream")]
    [InlineData("missing")]
    [InlineData("forbidden")]
    [InlineData("pending")]
    [InlineData("updating")]
    [InlineData("deleting")]
    public async Task HandleAsync_Should_AvoidStorage_When_TheRequestIsInvalidOrUnavailable(string condition)
    {
        var file = CreateFile(condition != "pending");
        if (condition == "updating")
        {
            file.BeginContentUpdate(FileVersionId.Create("initial").Value);
        }
        if (condition == "deleting")
        {
            file.BeginDeletion();
        }
        if (condition != "missing")
        {
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        }
        _user.HasPermission("file-manager.avatars.update").Returns(condition != "forbidden");
        await using var content = new MemoryStream([1]);

        var result = await CreateHandler().HandleAsync(
            new UpdateFileContentCommand(
                condition == "id" ? Guid.Empty : file.Id.Value,
                condition == "content-type" ? "text/*" : "text/plain",
                condition == "size" ? 0 : 1,
                condition == "stream" ? null! : content),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _storage.ReceivedCalls().ShouldBeEmpty();
        _unitOfWork.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Content update should avoid upload when reservation fails")]
    public async Task HandleAsync_Should_AvoidUpload_When_ReservationFails()
    {
        var file = CreateFile();
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _unitOfWork.When(work => work.SaveChangesAsync(Arg.Any<CancellationToken>()))
            .Do(_ => throw new IOException("Reservation failed."));
        await using var content = new MemoryStream([1]);

        var exception = await Record.ExceptionAsync(() => CreateHandler().HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 1, content),
            TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(
            default!, default!, default!, default!, TestContext.Current.CancellationToken);
    }

    private static FileMetadata UpdatedMetadata(File file)
    {
        return new FileMetadata("text/plain", 3,
            new Dictionary<string, string>
            {
                [FileMetadataNames.ContentUpdateId] = file.ContentUpdateId!.Value.ToString("D")
            }, "replacement");
    }

    private static File CreateFile(bool ready = true)
    {
        var file = File.Create(FileName.Create("original.txt").Value, FileType.Avatar, userId: null).Value;
        if (ready)
        {
            file.CompleteUpload(
                FileContentType.Create("text/plain").Value, FileSize.Create(4).Value,
                FileVersionId.Create("initial").Value);
        }
        return file;
    }

    private UpdateFileContentHandler CreateHandler()
    {
        return new UpdateFileContentHandler(_files, _storage, _unitOfWork, new FileAccessPolicy(_user));
    }
}
