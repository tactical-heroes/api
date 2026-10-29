using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.Delete;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class DeleteFileEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "Delete endpoint should persist intent before cleanup when file is accessible")]
    [InlineData("Personal", "PendingUpload")]
    [InlineData("Personal", "Ready")]
    [InlineData("Personal", "Deleting")]
    [InlineData("Avatar", "PendingUpload")]
    [InlineData("Avatar", "Ready")]
    [InlineData("Avatar", "Deleting")]
    public async Task HandleAsync_Should_PersistIntentBeforeCleanup_When_FileIsAccessible(string type, string status)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(type, userId, status);
        await UploadAsync(file);
        var permission = type == "Personal" ? "file-manager.personal-files.delete.own" : "file-manager.avatars.delete";

        using var response = await SendDeleteAsync(file.Id.Value, userId, permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        var stored = await ReadFileAsync(file);
        stored.Status.ShouldBe("Deleting");
        stored.DeletedAt.ShouldBeNull();
        (await GetMetadataAsync(file)).ShouldNotBeNull();
        using var repeated = await SendDeleteAsync(file.Id.Value, userId, permission);
        repeated.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}/download-url");
        downloadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        downloadRequest.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var download = await Fixture.Client.SendAsync(downloadRequest, TestContext.Current.CancellationToken);
        download.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        await ReconcileAsync();

        await AssertDeletedAsync(file);
        using var completed = await SendDeleteAsync(file.Id.Value, userId, permission);
        completed.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "Delete endpoint should honor all scope when another user owns the personal file")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_HonorAllScope_When_AnotherUserOwnsThePersonalFile(bool ownsFile)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", ownsFile ? userId : Guid.NewGuid(), "Ready");

        using var response = await SendDeleteAsync(file.Id.Value, userId, "file-manager.personal-files.delete.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await ReadFileAsync(file)).Status.ShouldBe("Deleting");
    }

    [Theory(DisplayName = "Delete endpoint should preserve file when matching delete permission is absent")]
    [InlineData("Personal", false, "file-manager.personal-files.delete.own")]
    [InlineData("Personal", true, "file-manager.personal-files.read.all")]
    [InlineData("Personal", true, "file-manager.personal-files.update.all")]
    [InlineData("Personal", true, "file-manager.avatars.delete")]
    [InlineData("Personal", true, "")]
    [InlineData("Avatar", false, "file-manager.personal-files.delete.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    [InlineData("Avatar", false, "")]
    public async Task HandleAsync_Should_PreserveFile_When_MatchingDeletePermissionIsAbsent(
        string type, bool ownsFile, string permission)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(type, ownsFile ? userId : Guid.NewGuid(), "Ready");
        await UploadAsync(file);

        using var response = await SendDeleteAsync(file.Id.Value, userId, permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var stored = await ReadFileAsync(file);
        stored.Status.ShouldBe("Ready");
        stored.DeletedAt.ShouldBeNull();
        (await GetMetadataAsync(file)).ShouldNotBeNull();
    }

    [Theory(DisplayName = "Delete endpoint should finish cleanup when ready content is missing or changed")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public async Task HandleAsync_Should_FinishCleanup_When_ReadyContentIsMissingOrChanged(string type, bool changed)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(type, userId, "Ready");
        if (changed)
        {
            await UploadAsync(file);
            await using var scope = Fixture.CreateScope();
            await using var content = new MemoryStream([5, 6, 7, 8, 9]);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .UploadAsync(file.StorageKey.Value, content, "image/png", TestContext.Current.CancellationToken);
        }

        using var response = await SendDeleteAsync(
            file.Id.Value,
            userId,
            type == "Personal" ? "file-manager.personal-files.delete.own" : "file-manager.avatars.delete");

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        await ReconcileAsync();
        await AssertDeletedAsync(file);
    }

    [Theory(DisplayName = "Delete endpoint should conceal file when id is unknown or soft deleted")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealFile_When_IdIsUnknownOrSoftDeleted(bool deleted)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", userId, "Ready");
        if (deleted)
        {
            await using var scope = Fixture.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var stored = await repository.GetByIdAsync(file.Id, TestContext.Current.CancellationToken);
            repository.Delete(stored!);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await SendDeleteAsync(
            deleted ? file.Id.Value : Guid.NewGuid(),
            userId,
            "file-manager.personal-files.delete.all");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Delete endpoint should reject caller when unauthenticated")]
    public async Task HandleAsync_Should_RejectCaller_When_Unauthenticated()
    {
        using var response = await SendDeleteAsync(
            Guid.NewGuid(),
            userId: null,
            "file-manager.personal-files.delete.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Delete endpoint should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_RejectIdentifier_When_IdIsEmpty()
    {
        using var response = await SendDeleteAsync(
            Guid.Empty,
            Guid.NewGuid(),
            "file-manager.personal-files.delete.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "Delete endpoint should preserve intent when a stale writer completes upload or renames")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveIntent_When_AStaleWriterCompletesUploadOrRenames(bool upload)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", userId, upload ? "PendingUpload" : "Ready");
        await using var staleScope = Fixture.CreateScope();
        var repository = staleScope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var stale = (await repository.GetByIdAsync(file.Id, TestContext.Current.CancellationToken))!;

        using var response = await SendDeleteAsync(file.Id.Value, userId, "file-manager.personal-files.delete.own");

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        if (upload)
        {
            stale.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value)
                .IsSuccess
                .ShouldBeTrue();
        }
        else
        {
            stale.Rename(FileName.Create("new.txt").Value).IsSuccess.ShouldBeTrue();
        }
        repository.Update(stale);
        await Should.ThrowAsync<ConcurrencyException>(() => UnitOfWork(staleScope.ServiceProvider)
            .SaveChangesAsync(TestContext.Current.CancellationToken));
        (await ReadFileAsync(file)).Status.ShouldBe("Deleting");
        await ReconcileAsync();
        await AssertDeletedAsync(file);
        await UploadAsync(file);
        await ReconcileAsync();
        await AssertDeletedAsync(file);
    }

    [Fact(DisplayName = "Delete endpoint should keep durable intent when storage is temporarily unavailable")]
    public async Task HandleAsync_Should_KeepDurableIntent_When_StorageIsTemporarilyUnavailable()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", userId, "Ready");
        using var client = Fixture.Storage.CreateClient();
        await client.DeleteBucketAsync(
            new Amazon.S3.Model.DeleteBucketRequest
            {
                BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
            },
            TestContext.Current.CancellationToken);
        try
        {
            using var response = await SendDeleteAsync(file.Id.Value, userId, "file-manager.personal-files.delete.own");

            response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
            await ReconcileAsync();
            var stored = await ReadFileAsync(file);
            stored.Status.ShouldBe("Deleting");
            stored.DeletedAt.ShouldBeNull();
        }
        finally
        {
            await client.PutBucketAsync(
                new Amazon.S3.Model.PutBucketRequest
                {
                    BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
                },
                TestContext.Current.CancellationToken);
        }

        await ReconcileAsync();

        await AssertDeletedAsync(file);
    }

    private static IUnitOfWork UnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
    }

    private async Task<HttpResponseMessage> SendDeleteAsync(Guid id, Guid? userId, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{id}");
        if (userId is { } subject)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject.ToString());
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(string type, Guid userId, string status)
    {
        var file = File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
        if (status == "Ready")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        }
        else if (status == "Deleting")
        {
            file.BeginDeletion();
        }
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IFilesRepository>().Add(file);
        await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(TestContext.Current.CancellationToken);
        return file;
    }

    private async Task UploadAsync(File file)
    {
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream([1, 2, 3, 4]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync(file.StorageKey.Value, content, "text/plain", TestContext.Current.CancellationToken);
    }

    private async Task<FileMetadata?> GetMetadataAsync(File file)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
    }

    private async Task<FileReadDbModel> ReadFileAsync(File file)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>().Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .SingleAsync(row => row.Id == file.Id.Value, TestContext.Current.CancellationToken);
    }

    private async Task ReconcileAsync()
    {
        await using var scope = Fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task AssertDeletedAsync(File file)
    {
        (await ReadFileAsync(file)).DeletedAt.ShouldNotBeNull();
        (await GetMetadataAsync(file)).ShouldBeNull();
        await using var scope = Fixture.CreateScope();
        var aggregate = await scope.ServiceProvider.GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, TestContext.Current.CancellationToken);
        aggregate.ShouldBeNull();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, Guid.NewGuid().ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            "file-manager.personal-files.read.all,file-manager.avatars.read");
        using var details = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
        details.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
