using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files;

public sealed class FileTests
{
    [Fact(DisplayName = "File should begin without content when type is known")]
    public void Create_Should_ReturnPendingUpload_When_TypeIsKnown()
    {
        var name = FileName.Create("avatar.png").Value;
        var fileType = FileType.Avatar;

        var file = File.Create(
            name,
            fileType,
            userId: null).Value;

        file.Id.Value.Version.ShouldBe(7);
        file.Name.ShouldBe(name);
        file.Type.ShouldBe(fileType);
        file.UserId.ShouldBeNull();
        file.FolderId.ShouldBeNull();
        file.Status.ShouldBe(FileStatus.PendingUpload);
        file.ContentType.ShouldBeNull();
        file.Size.ShouldBeNull();
    }

    [Theory(DisplayName = "File should require a user id only for personal files when ownership varies")]
    [InlineData("Avatar", false, true)]
    [InlineData("Avatar", true, false)]
    [InlineData("Personal", false, false)]
    [InlineData("Personal", true, true)]
    public void Create_Should_RequireUserIdOnlyForPersonalFiles_When_OwnershipVaries(
        string typeName, bool owned, bool valid)
    {
        UserId? userId = owned ? UserId.Create(Guid.CreateVersion7()).Value : null;

        var result = File.Create(
            name: FileName.Create("image.png").Value,
            type: FileType.FromName(typeName),
            userId: userId);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
        {
            result.Value.Type.Name.ShouldBe(typeName);
            result.Value.UserId.ShouldBe(userId);
            result.Value.FolderId.ShouldBeNull();
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.Validation, owned
                    ? "Only personal files can have a user id."
                    : "Personal files require a user id.")
                .ShouldHaveField(nameof(File.UserId));
        }
    }

    [Theory(DisplayName = "File should allow renaming only before deletion begins when status is provided")]
    [InlineData("PendingUpload", true)]
    [InlineData("Ready", true)]
    [InlineData("Deleting", false)]
    [InlineData("Deleted", false)]
    public void Rename_Should_RespectLifecycle_When_StatusIsProvided(string status, bool allowed)
    {
        var file = CreateFile(status);
        var name = FileName.Create("new-avatar.png").Value;

        var result = file.Rename(name);

        result.IsSuccess.ShouldBe(allowed);
        file.Name.Value.ShouldBe(allowed ? "new-avatar.png" : "avatar.png");
        file.Status.Name.ShouldBe(status);
        file.Type.ShouldBe(FileType.Avatar);
    }

    [Theory(DisplayName = "File should allow placement only before deletion begins when status is provided")]
    [InlineData("PendingUpload", true)]
    [InlineData("Ready", true)]
    [InlineData("Deleting", false)]
    [InlineData("Deleted", false)]
    public void MoveTo_Should_RespectLifecycle_When_StatusIsProvided(string status, bool allowed)
    {
        var file = CreateFile(status);
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;

        var result = file.MoveTo(folder);

        result.IsSuccess.ShouldBe(allowed);
        file.FolderId.ShouldBe(allowed ? folder.Id : null);
        file.Type.ShouldBe(FileType.Avatar);
        file.Status.Name.ShouldBe(status);
    }

    [Fact(DisplayName = "File should replace its folder reference when destination is changed")]
    public void MoveTo_Should_ReplaceFolderReference_When_DestinationIsChanged()
    {
        var file = CreateFile("Ready");
        var originalFolder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        var destination = originalFolder.CreateChild(FolderName.Create("Players").Value);
        file.MoveTo(originalFolder);

        var result = file.MoveTo(destination);

        result.IsSuccess.ShouldBeTrue();
        file.FolderId.ShouldBe(destination.Id);
        file.Type.ShouldBe(FileType.Avatar);
        file.Status.ShouldBe(FileStatus.Ready);
    }

    [Theory(DisplayName = "File should preserve placement for a different owner when destination owner varies")]
    [InlineData(true)]
    [InlineData(false)]
    public void MoveTo_Should_RequireMatchingUserId_When_DestinationOwnerVaries(bool sameOwner)
    {
        var userId = UserId.Create(Guid.CreateVersion7()).Value;
        var file = File.Create(
            name: FileName.Create("personal.png").Value,
            type: FileType.Personal,
            userId: userId).Value;
        var originalFolder = Folder.Create(
            name: FolderName.Create("Personal").Value,
            allowedFileType: FileType.Personal,
            userId: userId).Value;
        file.MoveTo(originalFolder).IsSuccess.ShouldBeTrue();
        var destination = Folder.Create(
            name: FolderName.Create("Documents").Value,
            allowedFileType: FileType.Personal,
            userId: sameOwner ? userId : UserId.Create(Guid.CreateVersion7()).Value).Value;

        var result = file.MoveTo(destination);

        result.IsSuccess.ShouldBe(sameOwner);
        file.FolderId.ShouldBe(sameOwner ? destination.Id : originalFolder.Id);
        file.UserId.ShouldBe(userId);
        if (!sameOwner)
        {
            result.ShouldHaveSingleError(ErrorType.Validation, "File and folder user ids must match.")
                .ShouldHaveField(nameof(File.FolderId));
        }
    }

    [Theory(DisplayName = "File should reject placement in a different category when file type varies")]
    [InlineData("Personal", "Avatar")]
    [InlineData("Avatar", "Personal")]
    public void MoveTo_Should_RejectDifferentType_When_FileTypeVaries(string fileType, string folderType)
    {
        var userId = UserId.Create(Guid.CreateVersion7()).Value;
        var file = File.Create(
            name: FileName.Create("image.png").Value,
            type: FileType.FromName(fileType),
            userId: fileType == "Personal" ? userId : null).Value;
        var folder = Folder.Create(
            name: FolderName.Create("Images").Value,
            allowedFileType: FileType.FromName(folderType),
            userId: folderType == "Personal" ? userId : null).Value;

        var result = file.MoveTo(folder);

        result.ShouldHaveSingleError(ErrorType.Validation, "File type must match the folder's allowed file type.")
            .ShouldHaveField(nameof(File.FolderId));
        file.FolderId.ShouldBeNull();
        file.UserId.ShouldBe(fileType == "Personal" ? userId : null);
    }

    [Fact(DisplayName = "File should become ready with verified content when upload is pending")]
    public void CompleteUpload_Should_SaveContent_When_UploadIsPending()
    {
        var file = CreateFile("PendingUpload");
        var contentType = FileContentType.Create("image/png").Value;
        var size = FileSize.Create(128).Value;

        var result = file.CompleteUpload(
            contentType,
            size);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType.ShouldBe(contentType);
        file.Size.ShouldBe(size);
    }

    [Fact(DisplayName = "File should accept repeated completion when completion is repeated")]
    public void CompleteUpload_Should_Succeed_When_CompletionIsRepeated()
    {
        var file = CreateFile("Ready");
        var contentType = FileContentType.Create("image/png").Value;
        var size = FileSize.Create(128).Value;

        var result = file.CompleteUpload(
            contentType,
            size);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType.ShouldBe(contentType);
        file.Size.ShouldBe(size);
    }

    [Theory(DisplayName = "File should preserve uploaded content when content differs")]
    [InlineData("image/webp", 128)]
    [InlineData("image/png", 129)]
    public void CompleteUpload_Should_ReturnConflict_When_ContentDiffers(
        string contentType, long size)
    {
        var file = CreateFile("Ready");
        var originalContentType = file.ContentType;
        var originalSize = file.Size;

        var result = file.CompleteUpload(
            FileContentType.Create(contentType).Value,
            FileSize.Create(size).Value);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File content cannot be replaced after upload.");
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType.ShouldBeSameAs(originalContentType);
        file.Size.ShouldBeSameAs(originalSize);
    }

    [Theory(DisplayName = "File should reject upload completion when deletion has begun")]
    [InlineData("Deleting")]
    [InlineData("Deleted")]
    public void CompleteUpload_Should_ReturnConflict_When_DeletionHasBegun(string status)
    {
        var file = CreateFile(status);
        var contentType = FileContentType.Create("image/png").Value;
        var size = FileSize.Create(128).Value;

        var result = file.CompleteUpload(
            contentType,
            size);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Only pending uploads can be completed.");
        file.Status.Name.ShouldBe(status);
        file.ContentType.ShouldBeNull();
        file.Size.ShouldBeNull();
    }

    [Theory(DisplayName = "File should begin deletion without restarting a completed deletion when status is provided")]
    [InlineData("PendingUpload", "Deleting")]
    [InlineData("Ready", "Deleting")]
    [InlineData("Deleting", "Deleting")]
    [InlineData("Deleted", "Deleted")]
    public void BeginDeletion_Should_RespectLifecycle_When_StatusIsProvided(
        string status, string expectedStatus)
    {
        var file = CreateFile(status);
        var contentType = file.ContentType;
        var size = file.Size;

        var result = file.BeginDeletion();

        result.IsSuccess.ShouldBeTrue();
        file.Status.Name.ShouldBe(expectedStatus);
        file.ContentType.ShouldBeSameAs(contentType);
        file.Size.ShouldBeSameAs(size);
    }

    [Theory(DisplayName = "File should complete deletion only after deletion has begun when status is provided")]
    [InlineData("PendingUpload", false)]
    [InlineData("Ready", false)]
    [InlineData("Deleting", true)]
    [InlineData("Deleted", true)]
    public void CompleteDeletion_Should_RespectLifecycle_When_StatusIsProvided(
        string status, bool allowed)
    {
        var file = CreateFile(status);
        var contentType = file.ContentType;
        var size = file.Size;

        var result = file.CompleteDeletion();

        result.IsSuccess.ShouldBe(allowed);
        file.Status.Name.ShouldBe(allowed ? "Deleted" : status);
        file.ContentType.ShouldBeSameAs(contentType);
        file.Size.ShouldBeSameAs(size);
    }

    private static File CreateFile(string status)
    {
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar,
            userId: null).Value;

        if (status == "Ready")
        {
            file.CompleteUpload(
                FileContentType.Create("image/png").Value,
                FileSize.Create(128).Value);
        }
        else if (status is "Deleting" or "Deleted")
        {
            file.BeginDeletion();
            if (status == "Deleted")
            {
                file.CompleteDeletion();
            }
        }

        return file;
    }
}
