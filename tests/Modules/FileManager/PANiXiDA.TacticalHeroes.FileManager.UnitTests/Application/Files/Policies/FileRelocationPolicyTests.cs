using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.UnitTests.Application.Files.Policies;

public sealed class FileRelocationPolicyTests
{
    [Theory(DisplayName = "Relocation policy should require verified content when metadata varies")]
    [InlineData("valid", true)]
    [InlineData("missing", false)]
    [InlineData("version", false)]
    [InlineData("size", false)]
    [InlineData("type", false)]
    [InlineData("marker", false)]
    [InlineData("attempt", false)]
    [InlineData("invalid-attempt", false)]
    public void CanAccept_Should_RequireVerifiedContent_When_MetadataVaries(string kind, bool accepted)
    {
        var attempt = FileOperationAttemptId.New();
        var marker = kind == "attempt" ? Guid.NewGuid().ToString() : attempt.Value.ToString();
        var values = new Dictionary<string, string>();
        if (kind != "marker")
        {
            values[FileMetadataNames.OperationAttemptId] = kind == "invalid-attempt" ? "invalid" : marker;
        }
        var metadata = kind == "missing" ? null : new FileMetadata(
            kind == "type" ? "image/png" : "text/plain",
            kind == "size" ? 9 : 4,
            values,
            kind == "version" ? null : "version");

        var result = FileRelocationPolicy.CanAccept(
            metadata,
            attempt,
            FileContentType.Create("text/plain").Value,
            FileSize.Create(4).Value);

        result.ShouldBe(accepted);
    }
}
