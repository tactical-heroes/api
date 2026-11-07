using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files;

public sealed class FileUpdateTests
{
    [Theory(DisplayName = "File update should preserve stored content when type and owner change")]
    [InlineData("Personal", "Personal", false)]
    [InlineData("Personal", "Avatar", true)]
    [InlineData("Avatar", "Personal", false)]
    [InlineData("Avatar", "Avatar", true)]
    public void Update_Should_PreserveStoredContent_When_TypeAndOwnerChange(
        string sourceType, string targetType, bool detach)
    {
        var file = CreateFile(sourceType);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(5).Value);
        var key = file.StorageKey;
        var owner = targetType == "Personal" ? UserId.Create(Guid.NewGuid()).Value : (UserId?)null;
        var type = FileType.FromName(targetType);
        var folder = Folder.Create(FolderName.Create("Target").Value, type, owner).Value;

        var result = file.Update(FileName.Create("renamed.txt").Value, type, owner, detach ? null : folder);

        result.IsSuccess.ShouldBeTrue();
        file.Name.Value.ShouldBe("renamed.txt");
        file.Type.ShouldBe(type);
        file.UserId.ShouldBe(owner);
        file.FolderId.ShouldBe(detach ? null : folder.Id);
        file.StorageKey.ShouldBeSameAs(key);
        file.Status.ShouldBe(FileStatus.Ready);
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(5);
    }

    [Theory(DisplayName = "File update should preserve all fields when proposed metadata is incompatible")]
    [InlineData("missing-owner")]
    [InlineData("unexpected-owner")]
    [InlineData("folder-type")]
    [InlineData("folder-owner")]
    [InlineData("deleting")]
    public void Update_Should_PreserveAllFields_When_ProposedMetadataIsIncompatible(string kind)
    {
        var file = CreateFile("Personal");
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
        var status = file.Status;
        var key = file.StorageKey;

        var result = file.Update(FileName.Create("renamed.txt").Value, type, owner, destination);

        result.IsFailure.ShouldBeTrue();
        result.Errors
            .ShouldAllBe(error => error.Type == (kind == "deleting" ? ErrorType.Conflict : ErrorType.Validation));
        file.Name.Value.ShouldBe("before.txt");
        file.Type.ShouldBe(FileType.Personal);
        file.UserId.ShouldBe(original.UserId);
        file.FolderId.ShouldBe(original.Id);
        file.Status.ShouldBe(status);
        file.StorageKey.ShouldBeSameAs(key);
    }

    private static File CreateFile(string type)
    {
        return File.Create(
            FileName.Create("before.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : null).Value;
    }
}
