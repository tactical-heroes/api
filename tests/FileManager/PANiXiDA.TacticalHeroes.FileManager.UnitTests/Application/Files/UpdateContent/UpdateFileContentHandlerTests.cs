using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
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
    }

    [Fact(DisplayName = "Content update should register the candidate when storage upload starts")]
    public async Task HandleAsync_Should_RegisterCandidate_When_StorageUploadStarts()
    {
        var file = CreateFile();
        var oldKey = file.StorageKey;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), "text/plain", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                file.StorageKey.ShouldBeSameAs(oldKey);
                file.UnusedContents.ShouldHaveSingleItem().StorageKey.ShouldNotBe(oldKey);
                _unitOfWork.ReceivedCalls().Count().ShouldBe(1);
                return Task.CompletedTask;
            });
        _storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 3));
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await CreateHandler().HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.StorageKey.ShouldNotBe(oldKey);
        file.UnusedContents.ShouldHaveSingleItem().StorageKey.ShouldBe(oldKey);
        await _unitOfWork.Received(2).SaveChangesAsync(TestContext.Current.CancellationToken);
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should retain old content when storage cannot provide a valid replacement")]
    [InlineData("upload-error")]
    [InlineData("metadata-error")]
    [InlineData("missing")]
    [InlineData("content-type")]
    [InlineData("size")]
    [InlineData("deleting")]
    public async Task HandleAsync_Should_RetainOldContent_When_StorageCannotProvideAValidReplacement(string condition)
    {
        var file = CreateFile();
        var oldKey = file.StorageKey;
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _storage.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                if (condition == "upload-error")
                {
                    throw new IOException("Upload failed.");
                }
                if (condition == "deleting")
                {
                    file.BeginDeletion();
                }
                return Task.CompletedTask;
            });
        _storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (condition == "metadata-error")
            {
                throw new IOException("Metadata failed.");
            }
            return condition == "missing" ? null : new FileMetadata(
                condition == "content-type" ? "image/png" : "text/plain", condition == "size" ? 99 : 3);
        });
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
        file.StorageKey.ShouldBeSameAs(oldKey);
        file.Size!.Value.ShouldBe(4);
        file.UnusedContents.ShouldHaveSingleItem().DeletionStartedAt.ShouldBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Content update should avoid storage when the request is invalid or unavailable")]
    [InlineData("id")]
    [InlineData("content-type")]
    [InlineData("size")]
    [InlineData("stream")]
    [InlineData("missing")]
    [InlineData("forbidden")]
    [InlineData("pending")]
    public async Task HandleAsync_Should_AvoidStorage_When_TheRequestIsInvalidOrUnavailable(string condition)
    {
        var file = CreateFile(condition != "pending");
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
        return new UpdateFileContentHandler(
            _files,
            _storage,
            _unitOfWork,
            new FileAccessPolicy(_user),
            TimeProvider.System);
    }
}
