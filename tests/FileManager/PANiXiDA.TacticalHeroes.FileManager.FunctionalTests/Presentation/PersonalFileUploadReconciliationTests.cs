using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Wolverine.Runtime;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

public sealed class PersonalFileUploadReconciliationTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Reconciliation should restore metadata when command is dispatched through mediator")]
    public async Task HandleAsync_Should_RestoreMetadata_When_CommandIsDispatchedThroughMediator()
    {
        var file = await CreatePendingUploadAsync();
        await using var scope = Fixture.CreateScope();

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("Ready");
        stored.Size.ShouldBe(4);
        stored.ContentType.ShouldBe("text/plain");
    }

    [Fact(DisplayName = "Reconciliation should run in background when recurring schedule fires")]
    public async Task ReconcileAsync_Should_RunInBackground_When_RecurringScheduleFires()
    {
        var file = await CreatePendingUploadAsync();

        var stored = await WaitForFileAsync(file.Id, item => item.Status == "Ready");

        stored.Status.ShouldBe("Ready");
        stored.Size.ShouldBe(4);
        stored.ContentType.ShouldBe("text/plain");
    }

    [Fact(DisplayName = "Reconciliation should soft delete an expired upload when recurring schedule fires")]
    public async Task ReconcileAsync_Should_SoftDeleteExpiredUpload_When_RecurringScheduleFires()
    {
        var file = await CreatePendingUploadAsync(uploadContent: false);

        var stored = await WaitForFileAsync(file.Id, item => item.DeletedAt != null);

        stored.Status.ShouldBe("Deleting");
        stored.DeletedAt.ShouldNotBeNull();
        stored.Size.ShouldBeNull();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/files/personal/{file.Id.Value}");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, file.UserId!.Value.Value.ToString());
        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await using var scope = Fixture.CreateScope();
        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        metadata.ShouldBeNull();
    }

    [Fact(DisplayName = "Reconciliation schedule should target the command when host is configured")]
    public void Scheduling_Should_TargetCommand_When_HostIsConfigured()
    {
        using var scope = Fixture.CreateScope();
        var runtime = scope.ServiceProvider.GetRequiredService<IWolverineRuntime>();

        var schedule = runtime.Options.Schedules.FindByName("file-manager-reconcile-personal-uploads");

        schedule.ShouldNotBeNull();
        schedule.MessageType.ShouldBe(typeof(ReconcilePersonalFileUploadsCommand));
        schedule.Schedule.Expression.ShouldBe("*/5 * * * * *");
        schedule.Schedule.TimeZone.Id.ShouldBe("UTC");
    }

    private async Task<File> CreatePendingUploadAsync(bool uploadContent = true)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = File.Create(
            FileName.Create("file.txt").Value,
            FileType.Personal,
            UserId.Create(Guid.NewGuid()).Value).Value;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        context.Add(file);
        await context.SaveChangesAsync(cancellationToken);
        if (uploadContent)
        {
            await using var content = new MemoryStream([1, 2, 3, 4]);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .UploadAsync(file.StorageKey.Value, content, "text/plain", cancellationToken);
        }

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch} WHERE id = {file.Id.Value}",
            cancellationToken);

        return file;
    }

    private async Task<FileReadDbModel> ReadFileAsync(FileId id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .SingleAsync(file => file.Id == id.Value, TestContext.Current.CancellationToken);
    }

    private async Task<FileReadDbModel> WaitForFileAsync(FileId id, Func<FileReadDbModel, bool> predicate)
    {
        var deadline = TimeProvider.System.GetUtcNow().AddSeconds(45);
        while (TimeProvider.System.GetUtcNow() < deadline)
        {
            var file = await ReadFileAsync(id);
            if (predicate(file))
            {
                return file;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("The recurring Wolverine command did not reach the expected file state.");
    }
}
