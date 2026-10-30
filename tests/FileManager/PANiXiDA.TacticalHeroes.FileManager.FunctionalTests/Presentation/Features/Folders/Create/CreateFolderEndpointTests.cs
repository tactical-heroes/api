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
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Create;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.Create;

public sealed class CreateFolderEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "POST folder should persist a root and return its location when creation is permitted")]
    [InlineData("Personal", "file-manager.personal-files.create.all", "self")]
    [InlineData("Personal", "file-manager.personal-files.create.own", "self")]
    [InlineData("Personal", "file-manager.personal-files.create.all", "foreign")]
    [InlineData("Avatar", "file-manager.avatars.create", "none")]
    public async Task HandleAsync_Should_PersistRootAndReturnLocation_When_CreationIsPermitted(
        string type, string permission, string ownership)
    {
        var userId = Guid.NewGuid();
        Guid? ownerId = ownership == "foreign" ? Guid.NewGuid() : ownership == "self" ? userId : null;
        var request = new CreateFolderRequest(" Documents ", type, ownerId);

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateFolderResponse>(
            TestContext.Current.CancellationToken);
        body.ShouldNotBeNull();
        body.Id.ShouldNotBe(Guid.Empty);
        response.Headers.Location.ShouldNotBeNull();
        var location = new Uri(Fixture.Client.BaseAddress!, response.Headers.Location);
        location.AbsolutePath.ShouldBe($"{Route}/{body.Id}");
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var stored = await db.Set<FolderReadDbModel>().SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(body.Id);
        stored.Name.ShouldBe("Documents");
        stored.AllowedFileType.ShouldBe(type);
        stored.UserId.ShouldBe(ownerId);
        stored.ParentId.ShouldBeNull();
        using var get = new HttpRequestMessage(HttpMethod.Get, location);
        get.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        get.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.all" : "file-manager.avatars.read");
        using var detailsResponse = await Fixture.Client.SendAsync(get, TestContext.Current.CancellationToken);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await detailsResponse.Content.ReadFromJsonAsync<GetFolderDetailsResponse>(
            TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.UserId.ShouldBe(stored.UserId);
    }

    [Theory(DisplayName = "POST folder should inherit parent metadata when child creation is permitted")]
    [InlineData("Personal", false, false)]
    [InlineData("Personal", false, true)]
    [InlineData("Personal", true, false)]
    [InlineData("Avatar", false, false)]
    public async Task HandleAsync_Should_InheritParentMetadata_When_ChildCreationIsPermitted(
        string type, bool foreignOwner, bool explicitOwner)
    {
        var userId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : userId : null;
        var parent = await AddParentAsync(type, ownerId);
        var request = new CreateFolderRequest("Child", type, explicitOwner ? ownerId : null, parent.Id.Value);
        var permission = type == "Personal"
            ? foreignOwner ? "file-manager.personal-files.create.all" : "file-manager.personal-files.create.own"
            : "file-manager.avatars.create";

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateFolderResponse>(
            TestContext.Current.CancellationToken);
        body.ShouldNotBeNull();
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        (await db.Set<FolderReadDbModel>().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
        var child = await db.Set<FolderReadDbModel>()
            .SingleAsync(folder => folder.Id == body.Id, TestContext.Current.CancellationToken);
        child.Name.ShouldBe("Child");
        child.AllowedFileType.ShouldBe(type);
        child.UserId.ShouldBe(ownerId);
        child.ParentId.ShouldBe(parent.Id.Value);
        var storedParent = await db.Set<FolderReadDbModel>()
            .SingleAsync(folder => folder.Id == parent.Id.Value, TestContext.Current.CancellationToken);
        storedParent.Name.ShouldBe("Parent");
        storedParent.ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "POST folder should return forbidden when permission does not authorize the root")]
    [InlineData("Personal", "file-manager.personal-files.create.own", true)]
    [InlineData("Personal", "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", "file-manager.avatars.create", false)]
    [InlineData("Personal", "", false)]
    [InlineData("Avatar", "file-manager.personal-files.create.all", false)]
    [InlineData("Avatar", "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_ReturnForbidden_When_PermissionDoesNotAuthorizeTheRoot(
        string type, string permission, bool foreignOwner)
    {
        var userId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : userId : null;
        var request = new CreateFolderRequest("Documents", type, ownerId);

        using var response = await SendAsync(request, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await AssertFolderCountAsync(0);
    }

    [Theory(DisplayName = "POST folder should conceal the parent when parent is missing deleted or forbidden")]
    [InlineData("missing")]
    [InlineData("deleted")]
    [InlineData("foreign")]
    [InlineData("avatar")]
    public async Task HandleAsync_Should_ConcealParent_When_ParentIsMissingDeletedOrForbidden(string kind)
    {
        var userId = Guid.NewGuid();
        var parent = await AddParentAsync(
            kind == "avatar" ? "Avatar" : "Personal",
            kind == "avatar" ? null : kind == "foreign" ? Guid.NewGuid() : userId,
            deleted: kind == "deleted");
        var request = new CreateFolderRequest(
            "Child", "Personal", ParentId: kind == "missing" ? Guid.NewGuid() : parent.Id.Value);

        using var response = await SendAsync(request, userId.ToString(), "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertFolderCountAsync(kind == "deleted" ? 0 : 1);
    }

    [Theory(DisplayName = "POST folder should reject inconsistent metadata when request conflicts with parent")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_Should_RejectInconsistentMetadata_When_RequestConflictsWithParent(bool wrongType)
    {
        var userId = Guid.NewGuid();
        var parent = await AddParentAsync("Personal", userId);
        var request = new CreateFolderRequest(
            "Child", wrongType ? "Avatar" : "Personal", wrongType ? null : Guid.NewGuid(), parent.Id.Value);

        using var response = await SendAsync(request, userId.ToString(), "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldContain(wrongType ? "AllowedFileType" : "UserId");
        await AssertFolderCountAsync(1);
    }

    [Theory(DisplayName = "POST folder should return bad request when metadata is invalid")]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("owner")]
    [InlineData("parent")]
    [InlineData("avatar-owner")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_MetadataIsInvalid(string invalid)
    {
        var request = new CreateFolderRequest(
            invalid == "name" ? "../folder" : "Documents",
            invalid == "type" ? "Other" : invalid == "avatar-owner" ? "Avatar" : "Personal",
            invalid == "owner" ? Guid.Empty : invalid == "avatar-owner" ? Guid.NewGuid() : null,
            invalid == "parent" ? Guid.Empty : null);

        using var response = await SendAsync(
            request,
            Guid.NewGuid().ToString(),
            "file-manager.personal-files.create.all,file-manager.avatars.create");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertFolderCountAsync(0);
    }

    [Fact(DisplayName = "POST folder should accept missing optional properties when only required fields are supplied")]
    public async Task HandleAsync_Should_AcceptMissingOptionalProperties_When_OnlyRequiredFieldsAreSupplied()
    {
        var request = new { Name = "Documents", AllowedFileType = "Avatar" };

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.avatars.create");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await AssertFolderCountAsync(1);
    }

    [Fact(DisplayName = "POST folder should reject missing ownership when personal root has no user id")]
    public async Task HandleAsync_Should_RejectMissingOwnership_When_PersonalRootHasNoUserId()
    {
        var request = new { Name = "Documents", AllowedFileType = "Personal" };

        using var response = await SendAsync(
            request, Guid.NewGuid().ToString(), "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldContain("Personal folders require a user id.");
        await AssertFolderCountAsync(0);
    }

    [Fact(DisplayName = "POST folder should require authentication when caller is unauthenticated")]
    public async Task HandleAsync_Should_RequireAuthentication_When_CallerIsUnauthenticated()
    {
        var request = new CreateFolderRequest("Documents", "Personal");

        using var response = await SendAsync(request, subject: null, "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertFolderCountAsync(0);
    }

    private async Task<HttpResponseMessage> SendAsync(object body, string? subject, string permissions)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Route) { Content = JsonContent.Create(body) };
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permissions);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddParentAsync(string type, Guid? userId, bool deleted = false)
    {
        var parent = Folder.Create(
            FolderName.Create("Parent").Value,
            FileType.FromName(type),
            userId is { } id ? UserId.Create(id).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        repository.Add(parent);
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
