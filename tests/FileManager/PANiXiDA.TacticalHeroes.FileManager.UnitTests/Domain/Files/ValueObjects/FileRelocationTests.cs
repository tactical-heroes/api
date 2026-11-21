using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileRelocationTests
{
    [Fact(DisplayName = "File relocation should preserve intended metadata when created")]
    public void Create_Should_PreserveIntendedMetadata_When_Created()
    {
        var attempt = FileOperationAttemptId.New();
        var name = FileName.Create("target.txt").Value;
        var owner = UserId.Create(Guid.NewGuid()).Value;
        var folder = FolderId.New();
        var key = FileStorageKey.Create($"personal/{owner.Value}/{Guid.NewGuid()}").Value;

        var relocation = FileRelocation.Create(attempt, name, FileType.Personal, owner, folder, key);

        relocation.AttemptId.ShouldBe(attempt);
        relocation.Name.ShouldBe(name);
        relocation.Type.ShouldBe(FileType.Personal);
        relocation.UserId.ShouldBe(owner);
        relocation.FolderId.ShouldBe(folder);
        relocation.StorageKey.ShouldBe(key);
        relocation.ShouldBe(FileRelocation.Create(attempt, name, FileType.Personal, owner, folder, key));
    }

    [Fact(DisplayName = "File relocation should describe its intent when converted to string")]
    public void ToString_Should_DescribeIntent_When_ConvertedToString()
    {
        var relocation = FileRelocation.Create(
            FileOperationAttemptId.New(),
            FileName.Create("a.txt").Value,
            FileType.Avatar,
            null,
            null,
            FileStorageKey.Create("avatar/file").Value);

        relocation.ToString().ShouldContain("FileRelocation");
        relocation.ToString().ShouldContain("a.txt");
    }
}
