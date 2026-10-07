using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files;

public sealed class FileTests
{
    [Fact(DisplayName = "File should begin without content when purpose is known")]
    public void Create_Should_ReturnPendingUpload_When_PurposeIsKnown()
    {
        var name = FileName.Create("avatar.png").Value;
        var filePurpose = FilePurpose.Avatar;

        var file = File.Create(
            name,
            filePurpose);

        file.Id.Value.Version.ShouldBe(7);
        file.Name.ShouldBe(name);
        file.Purpose.ShouldBe(filePurpose);
        file.Status.ShouldBe(FileStatus.PendingUpload);
        file.Content.ShouldBeNull();
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
        file.Purpose.ShouldBe(FilePurpose.Avatar);
    }

    [Fact(DisplayName = "File should become ready with verified content when upload is pending")]
    public void CompleteUpload_Should_SaveContent_When_UploadIsPending()
    {
        var file = CreateFile("PendingUpload");
        var content = CreateContent();

        var result = file.CompleteUpload(content);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.Content.ShouldBe(content);
    }

    [Fact(DisplayName = "File should accept repeated completion when completion is repeated")]
    public void CompleteUpload_Should_Succeed_When_CompletionIsRepeated()
    {
        var file = CreateFile("Ready");
        var content = CreateContent();

        var result = file.CompleteUpload(content);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(FileStatus.Ready);
        file.Content.ShouldBe(content);
    }

    [Theory(DisplayName = "File should preserve uploaded content when content differs")]
    [InlineData("image/webp", 128, 'a')]
    [InlineData("image/png", 129, 'a')]
    [InlineData("image/png", 128, 'b')]
    public void CompleteUpload_Should_ReturnConflict_When_ContentDiffers(
        string contentType, long size, char checksumCharacter)
    {
        var file = CreateFile("Ready");
        var originalContent = file.Content;
        var replacement = FileContent.Create(
            contentType,
            size,
            new string(checksumCharacter, 64)).Value;

        var result = file.CompleteUpload(replacement);

        result.ShouldHaveSingleError(ErrorType.Conflict, "File content cannot be replaced after upload.");
        file.Status.ShouldBe(FileStatus.Ready);
        file.Content.ShouldBeSameAs(originalContent);
    }

    [Theory(DisplayName = "File should reject upload completion when deletion has begun")]
    [InlineData("Deleting")]
    [InlineData("Deleted")]
    public void CompleteUpload_Should_ReturnConflict_When_DeletionHasBegun(string status)
    {
        var file = CreateFile(status);
        var content = CreateContent();

        var result = file.CompleteUpload(content);

        result.ShouldHaveSingleError(ErrorType.Conflict, "Only pending uploads can be completed.");
        file.Status.Name.ShouldBe(status);
        file.Content.ShouldBeNull();
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
        var content = file.Content;

        var result = file.BeginDeletion();

        result.IsSuccess.ShouldBeTrue();
        file.Status.Name.ShouldBe(expectedStatus);
        file.Content.ShouldBeSameAs(content);
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
        var content = file.Content;

        var result = file.CompleteDeletion();

        result.IsSuccess.ShouldBe(allowed);
        file.Status.Name.ShouldBe(allowed ? "Deleted" : status);
        file.Content.ShouldBeSameAs(content);
    }

    private static File CreateFile(string status)
    {
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            FilePurpose.Avatar);

        if (status == "Ready")
        {
            file.CompleteUpload(CreateContent());
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

    private static FileContent CreateContent()
    {
        return FileContent.Create(
            "image/png",
            128,
            new string('a', 64)).Value;
    }
}
