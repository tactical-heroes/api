using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files;

public sealed class FileTests
{
    [Theory(DisplayName = "File should generate a unique storage key when type and owner are provided")]
    [InlineData("Avatar")]
    [InlineData("Personal")]
    public void Create_Should_GenerateUniqueStorageKey_When_TypeAndOwnerAreProvided(string typeName)
    {
        var type = FileType.FromName(typeName);
        UserId? userId = type == FileType.Personal ? UserId.Create(Guid.CreateVersion7()).Value : null;
        var name = FileName.Create("same-name.png").Value;

        var file = File.Create(
            name,
            type,
            userId)
            .Value;
        var other = File.Create(
            name,
            type,
            userId)
            .Value;

        var expected = type == FileType.Personal
            ? $"personal/{userId!.Value.Value:D}/{file.Id.Value:D}"
            : $"avatar/{file.Id.Value:D}";
        file.StorageKey.Value.ShouldBe(expected);
        file.StorageKey.ShouldNotBe(other.StorageKey);
        file.FolderId.ShouldBeNull();
    }

    [Theory(DisplayName = "File should retain its storage key when metadata and lifecycle change")]
    [InlineData("Avatar")]
    [InlineData("Personal")]
    public void Rename_Should_PreserveStorageKey_When_MetadataAndLifecycleChange(string typeName)
    {
        var type = FileType.FromName(typeName);
        UserId? userId = type == FileType.Personal ? UserId.Create(Guid.CreateVersion7()).Value : null;
        var file = File.Create(
            FileName.Create("file.png").Value,
            type,
            userId)
            .Value;
        var key = file.StorageKey;
        var folder = Folder.Create(
            FolderName.Create("Original").Value,
            type,
            userId)
            .Value;
        var destination = folder.CreateChild(FolderName.Create("Destination").Value);

        file.Rename(FileName.Create("renamed.webp").Value).IsSuccess.ShouldBeTrue();
        file.MoveTo(folder).IsSuccess.ShouldBeTrue();
        file.MoveTo(destination).IsSuccess.ShouldBeTrue();
        file.CompleteUpload(
            FileContentType.Create("image/png").Value,
            FileSize.Create(128).Value,
            FileVersionId.Create("initial").Value)
            .IsSuccess
            .ShouldBeTrue();
        file.BeginDeletion().IsSuccess.ShouldBeTrue();

        file.StorageKey.ShouldBeSameAs(key);
    }

    [Fact(DisplayName = "File should begin without content when type is known")]
    public void Create_Should_ReturnPendingUpload_When_TypeIsKnown()
    {
        var name = FileName.Create("avatar.png").Value;
        var fileType = FileType.Avatar;

        var file = File.Create(
            name,
            fileType,
            userId: null)
            .Value;

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
        string typeName,
        bool owned,
        bool valid)
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
            result.ShouldHaveSingleError(
                ErrorType.Validation,
                owned
                    ? "Only personal files can have a user id."
                    : "Personal files require a user id.")
                .ShouldHaveField(nameof(File.UserId));
        }
    }

    [Fact(DisplayName = "File should return its download content type when upload is ready")]
    public void GetDownloadContentType_Should_ReturnContentType_When_UploadIsReady()
    {
        var file = CreateFile("Ready");
        var contentType = file.ContentType;

        var result = file.GetDownloadContentType();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(contentType);
        file.Status.ShouldBe(FileStatus.Ready);
    }

    [Theory(DisplayName = "File should reject downloading when file is not ready")]
    [InlineData("PendingUpload", false)]
    [InlineData("Deleting", false)]
    [InlineData("Deleting", true)]
    public void GetDownloadContentType_Should_ReturnConflict_When_FileIsNotReady(string status, bool uploaded)
    {
        var file = CreateFile(uploaded ? "Ready" : status);
        if (uploaded)
        {
            file.BeginDeletion();
        }

        var contentType = file.ContentType;

        var result = file.GetDownloadContentType();

        result.ShouldHaveSingleError(ErrorType.Conflict, "File is not ready for download.");
        file.Status.Name.ShouldBe(status);
        file.ContentType.ShouldBeSameAs(contentType);
    }

    [Theory(DisplayName = "File should allow renaming only before deletion begins when status is provided")]
    [InlineData("PendingUpload", true)]
    [InlineData("Ready", true)]
    [InlineData("Deleting", false)]
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
    public void MoveTo_Should_RespectLifecycle_When_StatusIsProvided(string status, bool allowed)
    {
        var file = CreateFile(status);
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null)
            .Value;

        var result = file.MoveTo(folder);

        result.IsSuccess.ShouldBe(allowed);
        file.FolderId.ShouldBe(allowed ? folder.Id : null);
        file.Type.ShouldBe(FileType.Avatar);
        file.Status.Name.ShouldBe(status);
    }

    [Theory(DisplayName = "File should detach only before deletion begins when destination is null")]
    [InlineData("PendingUpload", true)]
    [InlineData("Ready", true)]
    [InlineData("Deleting", false)]
    public void MoveTo_Should_DetachOnlyBeforeDeletionBegins_When_DestinationIsNull(string status, bool allowed)
    {
        var file = CreateFile(status == "Deleting" ? "Ready" : status);
        var folder = Folder.Create(FolderName.Create("Avatars").Value, FileType.Avatar, null).Value;
        file.MoveTo(folder).IsSuccess.ShouldBeTrue();
        if (status == "Deleting")
        {
            file.BeginDeletion();
        }
        var key = file.StorageKey;
        var contentType = file.ContentType;
        var size = file.Size;

        var result = file.MoveTo(null);

        result.IsSuccess.ShouldBe(allowed);
        file.FolderId.ShouldBe(allowed ? null : folder.Id);
        file.StorageKey.ShouldBeSameAs(key);
        file.ContentType.ShouldBeSameAs(contentType);
        file.Size.ShouldBeSameAs(size);
        file.Status.Name.ShouldBe(status);
    }

    [Fact(DisplayName = "File should replace its folder reference when destination is changed")]
    public void MoveTo_Should_ReplaceFolderReference_When_DestinationIsChanged()
    {
        var file = CreateFile("Ready");
        var originalFolder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null)
            .Value;
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
            userId: userId)
            .Value;
        var originalFolder = Folder.Create(
            name: FolderName.Create("Personal").Value,
            allowedFileType: FileType.Personal,
            userId: userId)
            .Value;
        file.MoveTo(originalFolder).IsSuccess.ShouldBeTrue();
        var destination = Folder.Create(
            name: FolderName.Create("Documents").Value,
            allowedFileType: FileType.Personal,
            userId: sameOwner ? userId : UserId.Create(Guid.CreateVersion7()).Value)
            .Value;

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
            userId: fileType == "Personal" ? userId : null)
            .Value;
        var folder = Folder.Create(
            name: FolderName.Create("Images").Value,
            allowedFileType: FileType.FromName(folderType),
            userId: folderType == "Personal" ? userId : null)
            .Value;

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
            size,
            FileVersionId.Create("initial").Value);

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
            size,
            FileVersionId.Create("initial").Value);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType.ShouldBe(contentType);
        file.Size.ShouldBe(size);
    }

    [Theory(DisplayName = "File should preserve uploaded content when content differs")]
    [InlineData("image/webp", 128)]
    [InlineData("image/png", 129)]
    public void CompleteUpload_Should_ReturnConflict_When_ContentDiffers(
        string contentType,
        long size)
    {
        var file = CreateFile("Ready");
        var originalContentType = file.ContentType;
        var originalSize = file.Size;

        var result = file.CompleteUpload(
            FileContentType.Create(contentType).Value,
            FileSize.Create(size).Value,
            FileVersionId.Create("initial").Value);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File content cannot be replaced after upload.");
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType.ShouldBeSameAs(originalContentType);
        file.Size.ShouldBeSameAs(originalSize);
    }

    [Theory(DisplayName = "File should reject upload completion when deletion has begun")]
    [InlineData("Deleting")]
    public void CompleteUpload_Should_ReturnConflict_When_DeletionHasBegun(string status)
    {
        var file = CreateFile(status);
        var contentType = FileContentType.Create("image/png").Value;
        var size = FileSize.Create(128).Value;

        var result = file.CompleteUpload(
            contentType,
            size,
            FileVersionId.Create("initial").Value);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Only pending uploads can be completed.");
        file.Status.Name.ShouldBe(status);
        file.ContentType.ShouldBeNull();
        file.Size.ShouldBeNull();
    }

    [Theory(DisplayName = "File should begin deletion idempotently when status is provided")]
    [InlineData("PendingUpload", "Deleting")]
    [InlineData("Ready", "Deleting")]
    [InlineData("Deleting", "Deleting")]
    public void BeginDeletion_Should_RespectLifecycle_When_StatusIsProvided(
        string status,
        string expectedStatus)
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

    [Fact(DisplayName = "Content update should retain the active version when update starts")]
    public void BeginContentUpdate_Should_RetainActiveContent_When_UpdateStarts()
    {
        var file = CreateFile("Ready");
        var key = file.StorageKey;
        var size = file.Size;
        var version = FileVersionId.Create("initial").Value;

        var result = file.BeginContentUpdate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.Version.ShouldBe(7);
        file.ContentUpdateAttemptId.ShouldBe(result.Value);
        file.VersionId.ShouldBe(version);
        file.StorageKey.ShouldBeSameAs(key);
        file.Size.ShouldBeSameAs(size);
        file.Status.ShouldBe(FileStatus.Updating);
        file.GetDownloadContentType().IsSuccess.ShouldBeTrue();
        file.BeginContentUpdate().IsFailure.ShouldBeTrue();
        file.Rename(FileName.Create("renamed.txt").Value).IsFailure.ShouldBeTrue();
        file.MoveTo(null).IsFailure.ShouldBeTrue();
        file.BeginDeletion().IsFailure.ShouldBeTrue();
        file.CompleteUpload(
            file.ContentType!,
            file.Size!,
            FileVersionId.Create("initial").Value)
            .IsFailure
            .ShouldBeTrue();
    }

    [Theory(DisplayName = "Content update should require its operation and a new version when update is completed")]
    [InlineData(true, true, true, true)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    public void CompleteContentUpdate_Should_RequireMatchingOperation_When_UpdateIsCompleted(
        bool updating,
        bool matching,
        bool newVersion,
        bool success)
    {
        var file = CreateFile("Ready");
        var key = file.StorageKey;
        var size = file.Size;
        var contentType = file.ContentType;
        var initial = FileVersionId.Create("initial").Value;
        file.CompleteUpload(contentType!, size!, initial);
        var operation = updating ? file.BeginContentUpdate().Value : FileContentUpdateAttemptId.New();
        var supplied = matching ? operation : FileContentUpdateAttemptId.New();
        var version = FileVersionId.Create(newVersion ? "replacement" : "initial").Value;

        var result = file.CompleteContentUpdate(
            supplied,
            FileContentType.Create("image/png").Value,
            FileSize.Create(9).Value,
            version);

        result.IsSuccess.ShouldBe(success);
        file.StorageKey.ShouldBe(key);
        file.ContentUpdateAttemptId.ShouldBe(success || !updating ? null : operation);
        file.VersionId.ShouldBe(success ? version : initial);
        file.Size.ShouldBe(success ? FileSize.Create(9).Value : size);
        file.ContentType.ShouldBe(success ? FileContentType.Create("image/png").Value : contentType);
        file.Status.ShouldBe(success || !updating ? FileStatus.Ready : FileStatus.Updating);
    }

    [Theory(DisplayName = "Content update should reject files when file is not ready")]
    [InlineData("PendingUpload")]
    [InlineData("Deleting")]
    public void BeginContentUpdate_Should_RejectFile_When_FileIsNotReady(string status)
    {
        var file = CreateFile(status);

        var result = file.BeginContentUpdate();

        result.IsFailure.ShouldBeTrue();
        file.Status.Name.ShouldBe(status);
        file.ContentUpdateAttemptId.ShouldBeNull();
    }

    [Theory(DisplayName = "Content update should restore active content when a matching attempt is cancelled")]
    [InlineData(true)]
    [InlineData(false)]
    public void CancelContentUpdate_Should_RestoreActiveContent_When_AMatchingAttemptIsCancelled(bool matching)
    {
        var file = CreateFile("Ready");
        var key = file.StorageKey;
        var contentType = file.ContentType;
        var size = file.Size;
        var version = FileVersionId.Create("initial").Value;
        var operation = file.BeginContentUpdate().Value;

        var result = file.CancelContentUpdate(matching ? operation : FileContentUpdateAttemptId.New());

        result.IsSuccess.ShouldBe(matching);
        file.StorageKey.ShouldBe(key);
        file.VersionId.ShouldBe(version);
        file.ContentType.ShouldBe(contentType);
        file.Size.ShouldBe(size);
        file.Status.ShouldBe(matching ? FileStatus.Ready : FileStatus.Updating);
        file.ContentUpdateAttemptId.ShouldBe(matching ? null : operation);
        if (matching)
        {
            var next = file.BeginContentUpdate().Value;
            next.ShouldNotBe(operation);
            file.CompleteContentUpdate(
                operation,
                contentType!,
                size!,
                FileVersionId.Create("late").Value)
                .IsFailure
                .ShouldBeTrue();
            file.CancelContentUpdate(operation).IsFailure.ShouldBeTrue();
        }
    }

    [Fact(DisplayName = "Upload completion should reject a different version when metadata is identical")]
    public void CompleteUpload_Should_RejectDifferentVersion_When_MetadataIsIdentical()
    {
        var file = CreateFile("Ready");
        var version = FileVersionId.Create("initial").Value;
        file.CompleteUpload(file.ContentType!, file.Size!, version);

        var result = file.CompleteUpload(
            file.ContentType!,
            file.Size!,
            FileVersionId.Create("different").Value);

        result.IsFailure.ShouldBeTrue();
        file.VersionId.ShouldBe(version);
    }

    [Theory(DisplayName = "File update should switch content atomically when type and owner change")]
    [InlineData("Personal", "Personal", false)]
    [InlineData("Personal", "Avatar", true)]
    [InlineData("Avatar", "Personal", false)]
    [InlineData("Avatar", "Avatar", true)]
    public void Update_Should_SwitchContentAtomically_When_TypeAndOwnerChange(
        string sourceType, string targetType, bool detach)
    {
        var file = CreateFileWithType(sourceType);
        var version = FileVersionId.Create("initial").Value;
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(5).Value, version);
        var key = file.StorageKey;
        var originalName = file.Name;
        var originalType = file.Type;
        var originalOwner = file.UserId;
        var owner = targetType == "Personal" ? UserId.Create(Guid.NewGuid()).Value : (UserId?)null;
        var type = FileType.FromName(targetType);
        var folder = Folder.Create(FolderName.Create("Target").Value, type, owner).Value;

        var result = file.Update(FileName.Create("renamed.txt").Value, type, owner, detach ? null : folder);

        result.IsSuccess.ShouldBeTrue();
        var relocated = sourceType != targetType || originalOwner != owner;
        if (relocated)
        {
            file.Status.ShouldBe(FileStatus.Relocating);
            file.StorageKey.ShouldBe(key);
            file.VersionId.ShouldBe(version);
            file.Name.ShouldBe(originalName);
            file.Type.ShouldBe(originalType);
            file.UserId.ShouldBe(originalOwner);
            file.Relocation.ShouldNotBeNull();
            file.GetDownloadContentType().IsSuccess.ShouldBeTrue();
            file.Rename(FileName.Create("blocked.txt").Value).IsFailure.ShouldBeTrue();
            file.MoveTo(null).IsFailure.ShouldBeTrue();
            file.BeginDeletion().IsFailure.ShouldBeTrue();
            file.BeginContentUpdate().IsFailure.ShouldBeTrue();
            file.Update(originalName, originalType, originalOwner, null).IsFailure.ShouldBeTrue();
            file.CompleteUpload(file.ContentType!, file.Size!, version).IsFailure.ShouldBeTrue();
            file.CompleteRelocation(
                file.Relocation.AttemptId,
                FileVersionId.Create("copied").Value,
                detach ? null : folder).IsSuccess.ShouldBeTrue();
        }

        file.Name.Value.ShouldBe("renamed.txt");
        file.Type.ShouldBe(type);
        file.UserId.ShouldBe(owner);
        file.FolderId.ShouldBe(detach ? null : folder.Id);
        file.StorageKey.Value.ShouldBe(type == FileType.Personal
            ? $"personal/{owner!.Value.Value}/{file.Id.Value}"
            : $"avatar/{file.Id.Value}");
        file.Status.ShouldBe(FileStatus.Ready);
        file.VersionId!.Value.ShouldBe(relocated ? "copied" : "initial");
        file.Relocation.ShouldBeNull();
        file.ContentUpdateAttemptId.ShouldBeNull();
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(5);
    }

    [Theory(DisplayName = "Relocation completion should require matching intent when destination or attempt changes")]
    [InlineData("success")]
    [InlineData("attempt")]
    [InlineData("missing-folder")]
    [InlineData("folder-owner")]
    [InlineData("folder-type")]
    public void CompleteRelocation_Should_RequireMatchingIntent_When_DestinationOrAttemptChanges(string kind)
    {
        var file = CreateFile("Ready");
        var owner = UserId.Create(Guid.NewGuid()).Value;
        var folder = Folder.Create(FolderName.Create("Target").Value, FileType.Personal, owner).Value;
        file.Update(FileName.Create("target.txt").Value, FileType.Personal, owner, folder).IsSuccess.ShouldBeTrue();
        var relocation = file.Relocation!;
        var key = file.StorageKey;
        var oldVersion = file.VersionId;
        var target = kind switch
        {
            "missing-folder" => null,
            "folder-owner" => Folder.Create(FolderName.Create("Other").Value, FileType.Personal,
                UserId.Create(Guid.NewGuid()).Value).Value,
            "folder-type" => Folder.Create(FolderName.Create("Other").Value, FileType.Avatar, null).Value,
            _ => folder
        };
        var success = kind == "success";

        var result = file.CompleteRelocation(
            kind == "attempt" ? FileRelocationAttemptId.New() : relocation.AttemptId,
            FileVersionId.Create("initial").Value,
            target);

        result.IsSuccess.ShouldBe(success);
        file.Status.ShouldBe(success ? FileStatus.Ready : FileStatus.Relocating);
        file.StorageKey.ShouldBe(success ? relocation.StorageKey : key);
        file.VersionId.ShouldBe(oldVersion);
        file.Relocation.ShouldBe(success ? null : relocation);
        if (success)
        {
            file.CompleteRelocation(relocation.AttemptId, oldVersion!, target).IsFailure.ShouldBeTrue();
        }
    }

    [Theory(DisplayName = "Relocation cancellation should preserve active content when attempts vary")]
    [InlineData(true)]
    [InlineData(false)]
    public void CancelRelocation_Should_PreserveActiveContent_When_AttemptsVary(bool matches)
    {
        var file = CreateFile("Ready");
        var key = file.StorageKey;
        var name = file.Name;
        file.Update(name, FileType.Personal, UserId.Create(Guid.NewGuid()).Value, null);
        var relocation = file.Relocation!;

        var result = file.CancelRelocation(matches ? relocation.AttemptId : FileRelocationAttemptId.New());

        result.IsSuccess.ShouldBe(matches);
        file.StorageKey.ShouldBe(key);
        file.VersionId!.Value.ShouldBe("initial");
        file.Name.ShouldBe(name);
        file.Type.ShouldBe(FileType.Avatar);
        file.Status.ShouldBe(matches ? FileStatus.Ready : FileStatus.Relocating);
        file.Relocation.ShouldBe(matches ? null : relocation);
        if (matches)
        {
            file.CancelRelocation(relocation.AttemptId).IsFailure.ShouldBeTrue();
        }
    }

    [Fact(DisplayName = "File update should reject scope changes when original upload is pending")]
    public void Update_Should_RejectScopeChanges_When_OriginalUploadIsPending()
    {
        var file = CreateFile("PendingUpload");
        var key = file.StorageKey;

        var result = file.Update(file.Name, FileType.Personal, UserId.Create(Guid.NewGuid()).Value, null);

        result.IsFailure.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.PendingUpload);
        file.Relocation.ShouldBeNull();
        file.StorageKey.ShouldBe(key);
    }

    [Theory(DisplayName = "File update should preserve all fields when proposed metadata is incompatible")]
    [InlineData("missing-owner")]
    [InlineData("unexpected-owner")]
    [InlineData("folder-type")]
    [InlineData("folder-owner")]
    [InlineData("deleting")]
    [InlineData("updating")]
    public void Update_Should_PreserveAllFields_When_ProposedMetadataIsIncompatible(string kind)
    {
        var file = CreateFileWithType("Personal");
        var original = Folder.Create(FolderName.Create("Original").Value, file.Type, file.UserId).Value;
        file.MoveTo(original);
        var type = kind == "unexpected-owner" ? FileType.Avatar : FileType.Personal;
        var owner = kind == "missing-owner" ? null : file.UserId;
        var destination = Folder.Create(
            FolderName.Create("Target").Value,
            kind == "folder-type" ? FileType.Avatar : FileType.Personal,
            kind == "folder-type" ? null : UserId.Create(Guid.NewGuid()).Value).Value;
        if (kind == "deleting")
        {
            file.BeginDeletion();
        }
        if (kind == "updating")
        {
            file.CompleteUpload(
                FileContentType.Create("text/plain").Value,
                FileSize.Create(5).Value,
                FileVersionId.Create("initial").Value);
            file.BeginContentUpdate();
        }
        var version = file.VersionId;
        var attempt = file.ContentUpdateAttemptId;
        var status = file.Status;
        var key = file.StorageKey;

        var result = file.Update(FileName.Create("renamed.txt").Value, type, owner, destination);

        result.IsFailure.ShouldBeTrue();
        var expectedErrorType = kind is "deleting" or "updating" ? ErrorType.Conflict : ErrorType.Validation;
        result.Errors.ShouldAllBe(error => error.Type == expectedErrorType);
        file.Name.Value.ShouldBe("before.txt");
        file.Type.ShouldBe(FileType.Personal);
        file.UserId.ShouldBe(original.UserId);
        file.FolderId.ShouldBe(original.Id);
        file.Status.ShouldBe(status);
        file.VersionId.ShouldBe(version);
        file.ContentUpdateAttemptId.ShouldBe(attempt);
        file.StorageKey.ShouldBeSameAs(key);
    }

    private static File CreateFile(string status)
    {
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar,
            userId: null)
            .Value;

        if (status == "Ready")
        {
            file.CompleteUpload(
                FileContentType.Create("image/png").Value,
                FileSize.Create(128).Value,
                FileVersionId.Create("initial").Value);
        }
        else if (status == "Deleting")
        {
            file.BeginDeletion();
        }

        return file;
    }

    private static File CreateFileWithType(string type)
    {
        return File.Create(
            FileName.Create("before.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : null).Value;
    }
}
