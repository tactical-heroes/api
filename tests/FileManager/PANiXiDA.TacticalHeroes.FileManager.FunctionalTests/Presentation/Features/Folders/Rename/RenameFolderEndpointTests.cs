using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Rename;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.Rename;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class RenameFolderEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "PATCH folder name should respect update scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.update.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.personal-files.create.own", false)]
    [InlineData("Personal", true, "file-manager.personal-files.delete.all", false)]
    [InlineData("Personal", true, "file-manager.avatars.update", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.update", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_RespectUpdateScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder(type, own ? userId : Guid.NewGuid());
        await SaveAsync(folder);

        using var response = await SendAsync(
            folder.Id.Value, new RenameFolderRequest(" Heroes "), userId.ToString(), permission);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe(allowed ? "Heroes" : "Documents");
        stored.AllowedFileType.ShouldBe(type);
        stored.UserId.ShouldBe(folder.UserId?.Value);
        stored.ParentId.ShouldBeNull();
        stored.DeletedAt.ShouldBeNull();
        if (allowed)
        {
            (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "PATCH folder name should preserve placement when folder has a parent and a child")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreservePlacement_When_FolderHasAParentAndAChild(string type)
    {
        var userId = Guid.NewGuid();
        var root = CreateFolder(type, userId);
        var folder = root.CreateChild(FolderName.Create("Nested").Value);
        var child = folder.CreateChild(FolderName.Create("Child").Value);
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.AddRange(root, folder, child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var permission = type == "Personal" ? "file-manager.personal-files.update.own" : "file-manager.avatars.update";

        using var response = await SendAsync(
            folder.Id.Value, new RenameFolderRequest("Heroes"), userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(folder.Id.Value);
        stored.Name.ShouldBe("Heroes");
        stored.ParentId.ShouldBe(root.Id.Value);
        stored.UserId.ShouldBe(folder.UserId?.Value);
        stored.AllowedFileType.ShouldBe(type);
        (await ReadAsync(root.Id.Value)).Name.ShouldBe("Documents");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{child.Id.Value}");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var detailsResponse = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await detailsResponse.Content.ReadFromJsonAsync<GetFolderDetailsResponse>(
            TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.ParentId.ShouldBe(folder.Id.Value);
        details.ParentName.ShouldBe("Heroes");
        details.Name.ShouldBe("Child");
    }

    [Theory(DisplayName = "PATCH folder name should preserve the name when request is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("folder/child")]
    [InlineData("folder\\child")]
    [InlineData("folder\n")]
    public async Task HandleAsync_Should_PreserveName_When_RequestIsInvalid(string? name)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        await SaveAsync(folder);

        using var response = await SendAsync(
            folder.Id.Value, new { Name = name }, userId.ToString(), "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Documents");
    }

    [Fact(DisplayName = "PATCH folder name should require the name when body omits it")]
    public async Task HandleAsync_Should_RequireName_When_BodyOmitsIt()
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        await SaveAsync(folder);

        using var response = await SendAsync(
            folder.Id.Value, new { }, userId.ToString(), "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Documents");
    }

    [Theory(DisplayName = "PATCH folder name should reject the request when target or account is unavailable")]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("empty", 400)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_RejectRequest_When_TargetOrAccountIsUnavailable(string kind, int code)
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        await SaveAsync(folder, deleted: kind == "deleted");

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : kind == "empty" ? Guid.Empty : folder.Id.Value,
            new RenameFolderRequest("Heroes"),
            kind == "anonymous" ? null : userId.ToString(),
            "file-manager.personal-files.update.own");

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Documents");
    }

    [Fact(DisplayName = "PATCH folder name should accept retries when same name is supplied again")]
    public async Task HandleAsync_Should_AcceptRetries_When_SameNameIsSuppliedAgain()
    {
        var userId = Guid.NewGuid();
        var folder = CreateFolder("Personal", userId);
        await SaveAsync(folder);
        var request = new RenameFolderRequest("Heroes");
        using var first = await SendAsync(
            folder.Id.Value, request, userId.ToString(), "file-manager.personal-files.update.own");

        using var second = await SendAsync(
            folder.Id.Value, request, userId.ToString(), "file-manager.personal-files.update.own");

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(folder.Id.Value)).Name.ShouldBe("Heroes");
    }

    private static Folder CreateFolder(string type, Guid userId)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
    }

    private async Task SaveAsync(Folder folder, bool deleted = false)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(folder);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            db.Remove(folder);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    private async Task<FolderReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        return await db.Set<FolderReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(folder => folder.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, object body, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Route}/{id}/name");
        request.Content = JsonContent.Create(body);
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
