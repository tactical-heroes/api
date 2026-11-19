using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
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
    private readonly IFileContentCleanupScheduler _cleanup = Substitute.For<IFileContentCleanupScheduler>();

    public UpdateFileContentHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.HasPermission("file-manager.avatars.update").Returns(true);
        _unitOfWork.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Func<CancellationToken, Task>>(0)(call.ArgAt<CancellationToken>(1)));
    }

    [Fact(DisplayName = "Content update should reserve a new key when upload starts")]
    public async Task HandleAsync_Should_ReserveNewKey_When_UploadStarts()
    {
        var file = CreateFile();
        var oldKey = file.StorageKey;
        FileStorageKey? candidate = null;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 3));
        _storage.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), "text/plain", Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                file.Status.ShouldBe(FileStatus.Updating);
                file.StorageKey.ShouldBeSameAs(oldKey);
                candidate = file.PendingStorageKey;
                candidate.ShouldNotBeNull();
                call.ArgAt<string>(0).ShouldBe(candidate.Value);
                file.Size!.Value.ShouldBe(4);
                _unitOfWork.ReceivedCalls().Count().ShouldBe(1);
                return Task.CompletedTask;
            });
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await CreateHandler().HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.StorageKey.ShouldBe(candidate);
        file.StorageKey.ShouldNotBe(oldKey);
        file.PendingStorageKey.ShouldBeNull();
        file.Status.ShouldBe(FileStatus.Ready);
        file.Size!.Value.ShouldBe(3);
        await _unitOfWork.Received(2).SaveChangesAsync(TestContext.Current.CancellationToken);
        await _cleanup.Received(1).ScheduleAsync(oldKey, TestContext.Current.CancellationToken);
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should retain active content when upload cannot be confirmed")]
    [InlineData("upload-error")]
    [InlineData("metadata-error")]
    [InlineData("save-error")]
    [InlineData("missing")]
    [InlineData("content-type")]
    [InlineData("size")]
    public async Task HandleAsync_Should_RetainActiveContent_When_UploadCannotBeConfirmed(string condition)
    {
        var file = CreateFile();
        var key = file.StorageKey;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (condition == "metadata-error")
            {
                throw new IOException("Metadata failed.");
            }

            return condition == "missing" ? null : new FileMetadata(
                condition == "content-type" ? "image/png" : "text/plain",
                condition == "size" ? 99 : 3);
        });
        if (condition == "upload-error")
        {
            _storage.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
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
        if (condition != "save-error")
        {
            file.StorageKey.ShouldBeSameAs(key);
            file.Status.ShouldBe(FileStatus.Updating);
            file.PendingStorageKey.ShouldNotBeNull();
            file.Size!.Value.ShouldBe(4);
            _cleanup.ReceivedCalls().ShouldBeEmpty();
        }
        await _unitOfWork.Received(condition == "save-error" ? 2 : 1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
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
            file.BeginContentUpdate();
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
            default!, default!, default!, TestContext.Current.CancellationToken);
    }

    private static File CreateFile(bool ready = true)
    {
        var file = File.Create(FileName.Create("original.txt").Value, FileType.Avatar, userId: null).Value;
        if (ready)
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        }
        return file;
    }

    private UpdateFileContentHandler CreateHandler()
    {
        return new UpdateFileContentHandler(_files, _storage, _unitOfWork, new FileAccessPolicy(_user), _cleanup);
    }
}
