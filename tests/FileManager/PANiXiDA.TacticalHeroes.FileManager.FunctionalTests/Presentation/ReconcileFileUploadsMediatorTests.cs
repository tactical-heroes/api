using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class ReconcileFileUploadsMediatorTests(ManualReconciliationTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Reconciliation should restore metadata when command is dispatched through mediator")]
    public async Task HandleAsync_Should_RestoreMetadata_When_CommandIsDispatchedThroughMediator()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = File.Create(
            FileName.Create("file.txt").Value,
            FileType.Personal,
            UserId.Create(Guid.NewGuid()).Value).Value;
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IConfiguration>()
                .GetValue<bool>("FileManager:Schedules:ReconcileFileUploadsCommand:Enabled")
                .ShouldBeFalse();
            var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            context.Add(file);
            await context.SaveChangesAsync(cancellationToken);
            await using var content = new MemoryStream([1, 2, 3, 4]);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .UploadAsync(file.StorageKey.Value, content, "text/plain", cancellationToken);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch} WHERE id = {file.Id.Value}",
                cancellationToken);
        }

        await using var mediatorScope = Fixture.CreateScope();
        var result = await mediatorScope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ReconcileFileUploadsCommand(), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await using var readScope = Fixture.CreateScope();
        var stored = await readScope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .SingleAsync(item => item.Id == file.Id.Value, cancellationToken);
        stored.Status.ShouldBe("Ready");
        stored.Size.ShouldBe(4);
        stored.ContentType.ShouldBe("text/plain");
    }
}
