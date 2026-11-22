using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.CreatePersonal;

public sealed class CreatePersonalFolderEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders/personal";

    [Theory(DisplayName = "POST personal folder should create owned metadata when creation is permitted")]
    [InlineData(false, "file-manager.personal-files.create.own")]
    [InlineData(true, "file-manager.personal-files.create.own")]
    [InlineData(false, "file-manager.personal-files.create.all")]
    [InlineData(true, "file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_CreateOwnedMetadata_When_CreationIsPermitted(bool nested, string permission)
    {
        var userId = Guid.NewGuid();
        var parent = nested ? await AddParentAsync("Personal", userId, depth: 2) : null;
        object request = parent is null
            ? new { Name = " Documents " }
            : new CreatePersonalFolderRequest(" Documents ", parent.Id.Value);

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreatePersonalFolderResponse>(
            TestContext.Current.CancellationToken);
        body.ShouldNotBeNull();
        body.Id.ShouldNotBe(Guid.Empty);
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.EnumerateObject().Select(property => property.Name).ShouldBe(["id"]);
        response.Headers.Location.ShouldNotBeNull();
        var location = new Uri(Fixture.Client.BaseAddress!, response.Headers.Location);
        location.AbsolutePath.ShouldBe($"/api/v1/folders/{body.Id}");
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var stored = await db.Set<FolderReadDbModel>()
            .SingleAsync(folder => folder.Id == body.Id, TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Documents");
        stored.AllowedFileType.ShouldBe("Personal");
        stored.UserId.ShouldBe(userId);
        stored.ParentId.ShouldBe(parent?.Id.Value);
        using var get = new HttpRequestMessage(HttpMethod.Get, location);
        get.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        get.Headers.Add(TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.read.own");
        using var detailsResponse = await Fixture.Client.SendAsync(get, TestContext.Current.CancellationToken);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await detailsResponse.Content.ReadFromJsonAsync<GetFolderDetailsResponse>(
            TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(body.Id);
        details.UserId.ShouldBe(userId);
        details.ParentName.ShouldBe(parent?.Name.Value);
    }

    [Theory(DisplayName = "POST personal folder should ignore forged metadata when extra fields are supplied")]
    [InlineData("file-manager.personal-files.create.own")]
    [InlineData("file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_IgnoreForgedMetadata_When_ExtraFieldsAreSupplied(string permission)
    {
        var userId = Guid.NewGuid();
        var request = new
        {
            Name = "Documents",
            UserId = Guid.NewGuid(),
            AllowedFileType = "Avatar",
            Type = "Avatar"
        };

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var stored = await db.Set<FolderReadDbModel>().SingleAsync(TestContext.Current.CancellationToken);
        stored.UserId.ShouldBe(userId);
        stored.AllowedFileType.ShouldBe("Personal");
    }

    [Theory(
        DisplayName = "POST personal folder should deny creation when permission does not authorize personal folders")]
    [InlineData("")]
    [InlineData("file-manager.avatars.create")]
    [InlineData("file-manager.personal-files.read.own")]
    [InlineData("file-manager.personal-files.update.own")]
    public async Task HandleAsync_Should_DenyCreation_When_PermissionDoesNotAuthorizePersonalFolders(string permission)
    {
        var request = new CreatePersonalFolderRequest("Documents");

        using var response = await SendAsync(request, Guid.NewGuid().ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await AssertFolderCountAsync(0);
    }

    [Theory(DisplayName = "POST personal folder should conceal the parent when parent is unavailable")]
    [InlineData("missing", false)]
    [InlineData("missing", true)]
    [InlineData("deleted", false)]
    [InlineData("deleted", true)]
    [InlineData("foreign", false)]
    [InlineData("foreign", true)]
    [InlineData("avatar", false)]
    [InlineData("avatar", true)]
    public async Task HandleAsync_Should_ConcealParent_When_ParentIsUnavailable(string kind, bool createAll)
    {
        var userId = Guid.NewGuid();
        var parent = await AddParentAsync(
            kind == "avatar" ? "Avatar" : "Personal",
            kind == "avatar" ? null : kind == "foreign" ? Guid.NewGuid() : userId,
            deleted: kind == "deleted");
        var request = new CreatePersonalFolderRequest(
            "Child", kind == "missing" ? Guid.NewGuid() : parent.Id.Value);
        var permission = createAll
            ? "file-manager.personal-files.create.all,file-manager.avatars.create"
            : "file-manager.personal-files.create.own";

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertFolderCountAsync(kind == "deleted" ? 0 : 1);
    }

    [Fact(DisplayName = "POST personal folder should preserve ownership when owner spoofing targets a foreign parent")]
    public async Task HandleAsync_Should_PreserveOwnership_When_OwnerSpoofingTargetsAForeignParent()
    {
        var ownerId = Guid.NewGuid();
        var parent = await AddParentAsync("Personal", ownerId);
        var request = new { Name = "Child", ParentId = parent.Id.Value, UserId = ownerId };

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertFolderCountAsync(1);
    }

    [Theory(DisplayName = "POST personal folder should reject invalid metadata when request is invalid")]
    [InlineData("", false)]
    [InlineData("../folder", false)]
    [InlineData("Documents", true)]
    public async Task HandleAsync_Should_RejectInvalidMetadata_When_RequestIsInvalid(string name, bool emptyParent)
    {
        var request = new CreatePersonalFolderRequest(name, emptyParent ? Guid.Empty : null);

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertFolderCountAsync(0);
    }

    [Fact(DisplayName = "POST personal folder should reject the request when name is missing")]
    public async Task HandleAsync_Should_RejectRequest_When_NameIsMissing()
    {
        var request = new { ParentId = (Guid?)null };

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertFolderCountAsync(0);
    }

    [Theory(DisplayName = "POST personal folder should require an account when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid-subject")]
    public async Task HandleAsync_Should_RequireAccount_When_SubjectIsUnavailable(string? subject)
    {
        var request = new CreatePersonalFolderRequest("Documents");

        using var response = await SendAsync(request, subject, "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertFolderCountAsync(0);
    }

    private async Task<HttpResponseMessage> SendAsync(object body, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Route) { Content = JsonContent.Create(body) };
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddParentAsync(string type, Guid? userId, bool deleted = false, int depth = 0)
    {
        var parent = Folder.Create(
            FolderName.Create("Parent").Value,
            FileType.FromName(type),
            userId is { } id ? UserId.Create(id).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        repository.Add(parent);
        for (var level = 0; level < depth; level++)
        {
            parent = parent.CreateChild(FolderName.Create($"Parent {level}").Value);
            repository.Add(parent);
        }
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            repository.Delete(parent);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return parent;
    }

    private async Task AssertFolderCountAsync(int expected)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var count = await db.Set<FolderReadDbModel>().CountAsync(TestContext.Current.CancellationToken);
        count.ShouldBe(expected);
    }
}
