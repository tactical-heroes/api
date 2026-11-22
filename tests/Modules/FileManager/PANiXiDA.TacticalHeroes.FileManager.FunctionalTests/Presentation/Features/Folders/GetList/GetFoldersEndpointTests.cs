using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Folders.GetList;

public sealed class GetFoldersEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/folders";

    [Theory(DisplayName = "Folder list endpoint should apply ownership before paging when personal permission varies")]
    [InlineData("file-manager.personal-files.read.own", 2, "Gamma")]
    [InlineData("file-manager.personal-files.read.all", 4, "Beta")]
    [InlineData("file-manager.personal-files.read.own,file-manager.personal-files.read.all", 4, "Beta")]
    public async Task HandleAsync_Should_ApplyOwnershipBeforePaging_When_PersonalPermissionVaries(
        string permission, long total, string name)
    {
        var userId = Guid.NewGuid();
        await AddFolderAsync("Alpha", "Personal", userId);
        await AddFolderAsync("Gamma", "Personal", userId);
        var foreignUserId = Guid.NewGuid();
        await AddFolderAsync("Beta", "Personal", foreignUserId);
        await AddFolderAsync("Delta", "Personal", Guid.NewGuid());
        await AddFolderAsync("Avatar", "Avatar", null);
        await AddFolderAsync("Aardvark", "Personal", userId, deleted: true);

        using var response = await SendAsync(
            "?allowedFileType=Personal&pageNumber=2&pageSize=1", userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(total);
        page.PageNumber.ShouldBe(2);
        page.PageSize.ShouldBe(1);
        var item = page.Items.ShouldHaveSingleItem();
        item.Name.ShouldBe(name);
        item.AllowedFileType.ShouldBe("Personal");
        item.ParentId.ShouldBeNull();
        item.ParentName.ShouldBeNull();
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.GetProperty("items")[0].EnumerateObject().Select(property => property.Name)
            .ShouldBe(["id", "name", "allowedFileType", "parentId", "parentName"], ignoreOrder: true);
    }

    [Fact(DisplayName = "Folder list endpoint should return avatars when avatar reading is allowed")]
    public async Task HandleAsync_Should_ReturnAvatars_When_AvatarReadingIsAllowed()
    {
        var userId = Guid.NewGuid();
        var avatar = await AddFolderAsync("avatar.png", "Avatar", null);
        await AddFolderAsync("personal.txt", "Personal", userId);

        using var response = await SendAsync("?allowedFileType=Avatar", userId.ToString(), "file-manager.avatars.read");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(1);
        var item = page.Items.ShouldHaveSingleItem();
        item.Id.ShouldBe(avatar.Id.Value);
        item.AllowedFileType.ShouldBe("Avatar");
    }

    [Fact(DisplayName = "Folder list endpoint should filter and sort metadata when search and sorting are provided")]
    public async Task HandleAsync_Should_FilterAndSortMetadata_When_SearchAndSortingAreProvided()
    {
        var userId = Guid.NewGuid();
        await AddFolderAsync("Alpha REPORT", "Personal", userId);
        await AddFolderAsync("Zeta report", "Personal", userId);
        await AddFolderAsync("Foreign report", "Personal", Guid.NewGuid());
        await AddFolderAsync("Other", "Personal", userId);

        using var response = await SendAsync(
            "?allowedFileType=%20Personal%20&search=%20RePoRt%20&fields=Name:desc",
            userId.ToString(),
            "file-manager.personal-files.read.own");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["Zeta report", "Alpha REPORT"]);
    }

    [Theory(DisplayName = "Folder list endpoint should forbid access when the matching read permission is absent")]
    [InlineData("Personal", "")]
    [InlineData("Avatar", "")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Avatar", "file-manager.personal-files.read.all")]
    [InlineData("Avatar", "file-manager.personal-files.read.own")]
    [InlineData("Personal", "file-manager.personal-files.read")]
    [InlineData("Personal", "file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_ForbidAccess_When_TheMatchingReadPermissionIsAbsent(
        string type, string permission)
    {
        var userId = Guid.NewGuid();

        using var response = await SendAsync($"?allowedFileType={type}", userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory(DisplayName = "Folder list endpoint should reject the request when query parameters are invalid")]
    [InlineData("?allowedFileType=")]
    [InlineData("?allowedFileType=Unknown")]
    [InlineData("?userId=00000000-0000-0000-0000-000000000000")]
    [InlineData("?userId=invalid")]
    [InlineData("?allowedFileType=Personal&search=ab")]
    [InlineData("?allowedFileType=Personal&pageNumber=0")]
    [InlineData("?allowedFileType=Personal&pageSize=201")]
    [InlineData("?allowedFileType=Personal&fields=CreatedAt:asc")]
    public async Task HandleAsync_Should_RejectRequest_When_QueryParametersAreInvalid(string query)
    {
        using var response = await SendAsync(query, Guid.NewGuid().ToString(), "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "Folder list endpoint should require a user account when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid")]
    public async Task HandleAsync_Should_RequireUserAccount_When_SubjectIsUnavailable(string? subject)
    {
        using var response = await SendAsync(
            "?allowedFileType=Personal",
            subject,
            "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Folder list endpoint should return an empty page when accessible folders are absent")]
    public async Task HandleAsync_Should_ReturnEmptyPage_When_AccessibleFoldersAreAbsent()
    {
        using var response = await SendAsync(
            "?allowedFileType=Personal", Guid.NewGuid().ToString(), "file-manager.personal-files.read.own");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(0);
        page.Items.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder list endpoint should combine accessible types when type is omitted")]
    [InlineData(
        "file-manager.personal-files.read.all,file-manager.avatars.read",
        6,
        "Charlie avatar",
        "Delta personal")]
    [InlineData(
        "file-manager.personal-files.read.own,file-manager.avatars.read",
        4,
        "Delta personal",
        "Foxtrot avatar")]
    public async Task HandleAsync_Should_CombineAccessibleTypes_When_TypeIsOmitted(
        string permissions, long total, string first, string second)
    {
        var userId = Guid.NewGuid();
        await AddFolderAsync("Alpha personal", "Personal", userId);
        await AddFolderAsync("Delta personal", "Personal", userId);
        await AddFolderAsync("Beta foreign", "Personal", Guid.NewGuid());
        await AddFolderAsync("Echo foreign", "Personal", Guid.NewGuid());
        await AddFolderAsync("Charlie avatar", "Avatar", null);
        await AddFolderAsync("Foxtrot avatar", "Avatar", null);
        await AddFolderAsync("Deleted personal", "Personal", userId, deleted: true);
        await AddFolderAsync("Deleted avatar", "Avatar", null, deleted: true);
        var pageNumber = total > 2 ? 2 : 1;

        using var response = await SendAsync(
            $"?pageNumber={pageNumber}&pageSize=2", userId.ToString(), permissions);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(total);
        page.Items.Select(item => item.Name).ShouldBe([first, second]);
    }

    [Theory(DisplayName = "Folder list endpoint should forbid the full selection when only one type is permitted")]
    [InlineData("file-manager.personal-files.read.all")]
    [InlineData("file-manager.personal-files.read.own")]
    [InlineData("file-manager.avatars.read")]
    public async Task HandleAsync_Should_ForbidFullSelection_When_OnlyOneTypeIsPermitted(string permission)
    {
        using var response = await SendAsync("", Guid.NewGuid().ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Folder list endpoint should forbid listing when type and read permissions are absent")]
    public async Task HandleAsync_Should_ForbidListing_When_TypeAndReadPermissionsAreAbsent()
    {
        using var response = await SendAsync("", Guid.NewGuid().ToString(), "file-manager.personal-files.create.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory(DisplayName = "Folder list endpoint should enforce owner selection when user id is supplied")]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    public async Task HandleAsync_Should_EnforceOwnerSelection_When_UserIdIsSupplied(bool all, bool own, bool allowed)
    {
        var userId = Guid.NewGuid();
        var ownerId = own ? userId : Guid.NewGuid();
        await AddFolderAsync("Selected owner", "Personal", ownerId);
        await AddFolderAsync("Other owner", "Personal", Guid.NewGuid());
        await AddFolderAsync("Avatar", "Avatar", null);
        var permissions = all ? "file-manager.personal-files.read.all" : "file-manager.personal-files.read.own";
        permissions += ",file-manager.avatars.read";

        using var response = await SendAsync(
            $"?userId={ownerId}&pageSize=1&pageNumber=2",
            userId.ToString(),
            permissions);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.OK : HttpStatusCode.Forbidden);
        if (allowed)
        {
            var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
                TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
            page.ShouldNotBeNull();
            page.TotalCount.ShouldBe(2);
            page.Items.ShouldHaveSingleItem().Name.ShouldBe("Selected owner");
        }
    }

    [Theory(DisplayName = "Folder list should return parent metadata when parent visibility varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ReturnParentMetadata_When_ParentVisibilityVaries(bool parentDeleted)
    {
        var userId = Guid.NewGuid();
        var parent = await AddFolderAsync("Parent", "Personal", userId);
        var child = parent.CreateChild(FolderName.Create("Child").Value);
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(child);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(TestContext.Current.CancellationToken);
            if (parentDeleted)
            {
                await using var deletionScope = Fixture.CreateScope();
                var repository = deletionScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
                var stored = await repository.GetByIdAsync(parent.Id, TestContext.Current.CancellationToken);
                repository.Delete(stored!);
                await deletionScope.ServiceProvider
                    .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                    .SaveChangesAsync(TestContext.Current.CancellationToken);
            }
        }

        using var response = await SendAsync(
            "?allowedFileType=Personal",
            userId.ToString(),
            "file-manager.personal-files.read.own");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FolderListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(parentDeleted ? 1 : 2);
        var item = page.Items.Single(folder => folder.Id == child.Id.Value);
        item.ParentId.ShouldBe(parent.Id.Value);
        item.ParentName.ShouldBe(parentDeleted ? null : parent.Name.Value);
        if (!parentDeleted)
        {
            var root = page.Items.Single(folder => folder.Id == parent.Id.Value);
            root.ParentId.ShouldBeNull();
            root.ParentName.ShouldBeNull();
        }
    }

    private async Task<HttpResponseMessage> SendAsync(string query, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Route + query);
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddFolderAsync(string name, string type, Guid? userId, bool deleted = false)
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
