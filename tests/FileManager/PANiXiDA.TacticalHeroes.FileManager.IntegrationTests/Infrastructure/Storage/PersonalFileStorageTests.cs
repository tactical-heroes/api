using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.Testing.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Storage;

public sealed class PersonalFileStorageTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Personal file upload should save ready metadata and content when s3 upload succeeds")]
    public async Task HandleAsync_Should_SaveReadyFile_When_S3UploadSucceeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await using var content = new MemoryStream([1, 2, 3, 4]);
        var command = new CreatePersonalFileCommand(
            "file.bin", "application/octet-stream", content.Length, content, Guid.NewGuid(), FolderId: null);

        var result = await CreateHandler(scope.ServiceProvider).HandleAsync(command, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        unitOfWork.HasActiveTransaction.ShouldBeFalse();
        await using var verification = Fixture.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var file = await db.Set<FileReadDbModel>().SingleAsync(cancellationToken);
        file.Id.ShouldBe(result.Value);
        file.Status.ShouldBe("Ready");
        file.Size.ShouldBe(content.Length);
        var storage = verification.ServiceProvider.GetRequiredService<IFileStorage>();
        await using var downloaded = await storage.DownloadAsync(file.StorageKey, cancellationToken);
        await using var bytes = new MemoryStream();
        await downloaded.CopyToAsync(bytes, cancellationToken);
        bytes.ToArray().ShouldBe(content.ToArray());
    }

    [Fact(DisplayName = "Personal file upload should preserve committed pending metadata when bucket is missing")]
    public async Task HandleAsync_Should_KeepPendingRecord_When_BucketIsMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var s3 = Fixture.Storage.CreateClient();
        await s3.DeleteBucketAsync(new DeleteBucketRequest { BucketName = S3TestStorage.BucketName }, cancellationToken);

        try
        {
            await using var scope = Fixture.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await using var content = new MemoryStream([1]);
            var command = new CreatePersonalFileCommand(
                "file.bin", "application/octet-stream", content.Length, content, Guid.NewGuid(), FolderId: null);

            await Should.ThrowAsync<AmazonS3Exception>(() =>
                CreateHandler(scope.ServiceProvider).HandleAsync(command, cancellationToken));
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            await using var verification = Fixture.CreateScope();
            var db = verification.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
            var pending = await db.Set<FileReadDbModel>().SingleAsync(cancellationToken);
            pending.Status.ShouldBe("PendingUpload");
            pending.UserId.ShouldBe(command.UserId);
            pending.StorageKey.ShouldBe($"personal/{command.UserId:D}/{pending.Id:D}");
            pending.Size.ShouldBeNull();
            pending.ContentType.ShouldBeNull();
        }
        finally
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = S3TestStorage.BucketName }, cancellationToken);
        }
    }

    private static CreatePersonalFileHandler CreateHandler(IServiceProvider services)
    {
        return new CreatePersonalFileHandler(
            services.GetRequiredService<IFilesRepository>(),
            services.GetRequiredService<IFoldersRepository>(),
            services.GetRequiredService<IFileStorage>(),
            services.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext)));
    }
}
