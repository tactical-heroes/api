using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.GetDetails;

public sealed class GetFolderDetailsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "GET folder should return metadata when reading is permitted")]
    [InlineData("Personal", "file-manager.personal-files.read.own")]
    [InlineData("Avatar", "file-manager.avatars.read")]
    public async Task HandleAsync_Should_ReturnMetadata_When_ReadingIsPermitted(string type, string permission)
    {
        var userId = Guid.NewGuid();
        var folder = await AddFolderAsync(type, type == "Personal" ? userId : null);

        using var response = await SendAsync(folder.Id.Value, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content
            .ReadFromJsonAsync<GetFolderDetailsResponse>(TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(folder.Id.Value);
        details.Name.ShouldBe(folder.Name.Value);
        details.AllowedFileType.ShouldBe(type);
        details.UserId.ShouldBe(folder.UserId?.Value);
        details.ParentId.ShouldBeNull();
        details.ParentName.ShouldBeNull();
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.EnumerateObject().Select(property => property.Name)
            .ShouldBe(["id", "name", "allowedFileType", "userId", "parentId", "parentName"], ignoreOrder: true);
    }

    [Fact(DisplayName = "GET folder should allow another owner when read all is granted")]
    public async Task HandleAsync_Should_AllowAnotherOwner_When_ReadAllIsGranted()
    {
        var folder = await AddFolderAsync("Personal", Guid.NewGuid());

        using var response = await SendAsync(
            folder.Id.Value,
            Guid.NewGuid().ToString(),
            "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content
            .ReadFromJsonAsync<GetFolderDetailsResponse>(TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(folder.Id.Value);
        details.AllowedFileType.ShouldBe("Personal");
        details.UserId.ShouldBe(folder.UserId?.Value);
    }

    [Theory(DisplayName = "GET folder should conceal metadata when folder is missing deleted or forbidden")]
    [InlineData("missing", "file-manager.personal-files.read.all")]
    [InlineData("deleted", "file-manager.personal-files.read.all")]
    [InlineData("foreign", "file-manager.personal-files.read.own")]
    [InlineData("personal", "file-manager.avatars.read")]
    [InlineData("avatar", "file-manager.personal-files.read.all")]
    [InlineData("personal", "")]
    public async Task HandleAsync_Should_ConcealMetadata_When_FolderIsMissingDeletedOrForbidden(
        string kind,
        string permission)
    {
        var userId = Guid.NewGuid();
        var folder = await AddFolderAsync(
            kind == "avatar" ? "Avatar" : "Personal",
            kind == "avatar" ? null : kind == "foreign" ? Guid.NewGuid() : userId,
            deleted: kind == "deleted");

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : folder.Id.Value,
            userId.ToString(),
            permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET folder should use stored ownership when query parameters spoof access metadata")]
    public async Task HandleAsync_Should_UseStoredOwnership_When_QueryParametersSpoofAccessMetadata()
    {
        var userId = Guid.NewGuid();
        var folder = await AddFolderAsync("Personal", Guid.NewGuid());
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{Route}/{folder.Id.Value}?userId={userId}&allowedFileType=Avatar");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            "file-manager.avatars.read,file-manager.personal-files.read.own");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET folder should return unauthorized when caller is unauthenticated")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_CallerIsUnauthenticated()
    {
        using var response = await SendAsync(Guid.NewGuid(), subject: null, "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET folder should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_IdIsEmpty()
    {
        using var response = await SendAsync(
            Guid.Empty,
            Guid.NewGuid().ToString(),
            "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "GET folder should return parent metadata when parent visibility and type vary")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public async Task HandleAsync_Should_ReturnParentMetadata_When_ParentVisibilityAndTypeVary(
        string type, bool parentDeleted)
    {
        var userId = Guid.NewGuid();
        var parent = await AddFolderAsync(type, type == "Personal" ? userId : null, name: "Parent");
        var child = parent.CreateChild(FolderName.Create("Child").Value);
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(child);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        if (parentDeleted)
        {
            await using var scope = Fixture.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var stored = await repository.GetByIdAsync(parent.Id, TestContext.Current.CancellationToken);
            repository.Delete(stored!);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await SendAsync(
            child.Id.Value,
            userId.ToString(),
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content.ReadFromJsonAsync<GetFolderDetailsResponse>(
            TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(child.Id.Value);
        details.Name.ShouldBe("Child");
        details.AllowedFileType.ShouldBe(type);
        details.UserId.ShouldBe(child.UserId?.Value);
        details.ParentId.ShouldBe(parent.Id.Value);
        details.ParentName.ShouldBe(parentDeleted ? null : "Parent");
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{id}");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddFolderAsync(
        string type,
        Guid? userId,
        bool deleted = false,
        string name = "Documents")
    {
        var folder = Folder.Create(
            FolderName.Create(name).Value,
            FileType.FromName(type),
            userId is { } owner ? UserId.Create(owner).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        repository.Add(folder);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            repository.Delete(folder);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return folder;
    }
}
