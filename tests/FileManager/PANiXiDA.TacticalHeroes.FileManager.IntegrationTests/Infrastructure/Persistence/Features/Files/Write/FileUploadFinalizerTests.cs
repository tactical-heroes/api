using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Write;

public sealed class FileUploadFinalizerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Upload finalization should respect persisted lifecycle when status varies")]
    [InlineData("PendingUpload", true)]
    [InlineData("Ready", true)]
    [InlineData("Deleting", false)]
    [InlineData("Deleted", false)]
    public async Task CompleteAsync_Should_RespectPersistedLifecycle_When_StatusVaries(string status, bool succeeds)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var file = CreateFile();
        var contentType = FileContentType.Create("text/plain").Value;
        var size = FileSize.Create(4).Value;
        if (status != "PendingUpload")
        {
            file.CompleteUpload(contentType, size);
        }

        if (status is "Deleting" or "Deleted")
        {
            file.BeginDeletion();
        }

        if (status == "Deleted")
        {
            file.CompleteDeletion();
        }

        context.Add(file);
        await context.SaveChangesAsync(cancellationToken);

        var result = await scope.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
            .CompleteAsync(file.Id, contentType, size, cancellationToken);

        result.IsSuccess.ShouldBe(succeeds);
        await context.Entry(file).ReloadAsync(cancellationToken);
        file.Status.Name.ShouldBe(succeeds ? "Ready" : status);
    }

    [Fact(DisplayName = "Upload finalization should reload tracked metadata when another scope has already finalized")]
    public async Task CompleteAsync_Should_ReloadTrackedMetadata_When_AnotherScopeHasAlreadyFinalized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var uploading = Fixture.CreateScope();
        var context = uploading.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var file = CreateFile();
        context.Add(file);
        await context.SaveChangesAsync(cancellationToken);
        var contentType = FileContentType.Create("text/plain").Value;
        var size = FileSize.Create(4).Value;
        await using (var recovery = Fixture.CreateScope())
        {
            var recovered = await recovery.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
                .CompleteAsync(file.Id, contentType, size, cancellationToken);
            recovered.IsSuccess.ShouldBeTrue();
        }

        file.Status.Name.ShouldBe("PendingUpload");

        var result = await uploading.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
            .CompleteAsync(file.Id, contentType, size, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.Status.Name.ShouldBe("Ready");
        file.Size.ShouldBe(size);
        file.ContentType.ShouldBe(contentType);
    }

    [Fact(DisplayName = "Upload finalization should succeed idempotently when requests run concurrently")]
    public async Task CompleteAsync_Should_SucceedIdempotently_When_RequestsRunConcurrently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await using (var scope = Fixture.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            context.Add(file);
            await context.SaveChangesAsync(cancellationToken);
        }

        var results = await Task.WhenAll(FinalizeAsync(file.Id), FinalizeAsync(file.Id));

        results.ShouldAllBe(result => result.IsSuccess);
    }

    [Fact(DisplayName = "Upload finalization should preserve ready metadata when replacement metadata differs")]
    public async Task CompleteAsync_Should_PreserveReadyMetadata_When_ReplacementMetadataDiffers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        context.Add(file);
        await context.SaveChangesAsync(cancellationToken);

        var result = await scope.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
            .CompleteAsync(
                file.Id,
                FileContentType.Create("image/png").Value,
                FileSize.Create(8).Value,
                cancellationToken);

        result.IsFailure.ShouldBeTrue();
        await context.Entry(file).ReloadAsync(cancellationToken);
        file.Size!.Value.ShouldBe(4);
        file.ContentType!.Value.ShouldBe("text/plain");
    }

    [Fact(DisplayName = "Upload finalization should return not found when file is missing")]
    public async Task CompleteAsync_Should_ReturnNotFound_When_FileIsMissing()
    {
        await using var scope = Fixture.CreateScope();

        var result = await scope.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
            .CompleteAsync(
                FileId.New(),
                FileContentType.Create("text/plain").Value,
                FileSize.Create(4).Value,
                TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
    }

    private static File CreateFile()
    {
        return File.Create(
            FileName.Create("file.txt").Value,
            FileType.Personal,
            UserId.Create(Guid.NewGuid()).Value).Value;
    }

    private async Task<Result> FinalizeAsync(FileId id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IFileUploadFinalizer>()
            .CompleteAsync(
                id,
                FileContentType.Create("text/plain").Value,
                FileSize.Create(4).Value,
                TestContext.Current.CancellationToken);
    }
}
