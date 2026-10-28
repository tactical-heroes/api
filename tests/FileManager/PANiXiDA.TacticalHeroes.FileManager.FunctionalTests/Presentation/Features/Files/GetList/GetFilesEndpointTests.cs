using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetList;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.GetList;

public sealed class GetFilesEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "File list endpoint should apply ownership before paging when personal permission varies")]
    [InlineData("file-manager.personal-files.read.own", 2, "Gamma")]
    [InlineData("file-manager.personal-files.read.all", 4, "Beta")]
    [InlineData("file-manager.personal-files.read.own,file-manager.personal-files.read.all", 4, "Beta")]
    public async Task HandleAsync_Should_ApplyOwnershipBeforePaging_When_PersonalPermissionVaries(
        string permission, long total, string name)
    {
        var userId = Guid.NewGuid();
        await AddFileAsync("Alpha", "Personal", userId);
        await AddFileAsync("Gamma", "Personal", userId);
        var foreignUserId = Guid.NewGuid();
        await AddFileAsync("Beta", "Personal", foreignUserId);
        await AddFileAsync("Delta", "Personal", Guid.NewGuid());
        await AddFileAsync("Avatar", "Avatar", null);
        await AddFileAsync("Aardvark", "Personal", userId, deleted: true);

        using var response = await SendAsync(
            $"?type=Personal&pageNumber=2&pageSize=1&userId={foreignUserId}", userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FileListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(total);
        page.PageNumber.ShouldBe(2);
        page.PageSize.ShouldBe(1);
        var item = page.Items.ShouldHaveSingleItem();
        item.Name.ShouldBe(name);
        item.Type.ShouldBe("Personal");
        item.Status.ShouldBe("Ready");
        item.ContentType.ShouldBe("text/plain");
        item.Size.ShouldBe(4);
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.GetProperty("items")[0].EnumerateObject().Select(property => property.Name)
            .ShouldBe(["id", "name", "type", "status", "contentType", "size"], ignoreOrder: true);
    }

    [Fact(DisplayName = "File list endpoint should return avatars when avatar reading is allowed")]
    public async Task HandleAsync_Should_ReturnAvatars_When_AvatarReadingIsAllowed()
    {
        var userId = Guid.NewGuid();
        var avatar = await AddFileAsync("avatar.png", "Avatar", null);
        await AddFileAsync("personal.txt", "Personal", userId);

        using var response = await SendAsync("?type=Avatar", userId.ToString(), "file-manager.avatars.read");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FileListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(1);
        var item = page.Items.ShouldHaveSingleItem();
        item.Id.ShouldBe(avatar.Id.Value);
        item.Type.ShouldBe("Avatar");
    }

    [Fact(DisplayName = "File list endpoint should filter and sort metadata when search and sorting are provided")]
    public async Task HandleAsync_Should_FilterAndSortMetadata_When_SearchAndSortingAreProvided()
    {
        var userId = Guid.NewGuid();
        await AddFileAsync("Alpha REPORT", "Personal", userId);
        await AddFileAsync("Zeta report", "Personal", userId);
        await AddFileAsync("Foreign report", "Personal", Guid.NewGuid());
        await AddFileAsync("Other", "Personal", userId);

        using var response = await SendAsync(
            "?type=%20Personal%20&search=%20RePoRt%20&fields=Name:desc",
            userId.ToString(),
            "file-manager.personal-files.read.own");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FileListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["Zeta report", "Alpha REPORT"]);
    }

    [Theory(DisplayName = "File list endpoint should forbid access when the matching read permission is absent")]
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

        using var response = await SendAsync($"?type={type}", userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory(DisplayName = "File list endpoint should reject the request when query parameters are invalid")]
    [InlineData("")]
    [InlineData("?type=")]
    [InlineData("?type=Unknown")]
    [InlineData("?type=Personal&search=ab")]
    [InlineData("?type=Personal&pageNumber=0")]
    [InlineData("?type=Personal&pageSize=201")]
    [InlineData("?type=Personal&fields=CreatedAt:asc")]
    public async Task HandleAsync_Should_RejectRequest_When_QueryParametersAreInvalid(string query)
    {
        using var response = await SendAsync(query, Guid.NewGuid().ToString(), "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "File list endpoint should require a user account when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid")]
    public async Task HandleAsync_Should_RequireUserAccount_When_SubjectIsUnavailable(string? subject)
    {
        using var response = await SendAsync("?type=Personal", subject, "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "File list endpoint should return an empty page when accessible files are absent")]
    public async Task HandleAsync_Should_ReturnEmptyPage_When_AccessibleFilesAreAbsent()
    {
        using var response = await SendAsync(
            "?type=Personal", Guid.NewGuid().ToString(), "file-manager.personal-files.read.own");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<FileListItemResponse>>(
            TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(0);
        page.Items.ShouldBeEmpty();
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

    private async Task<File> AddFileAsync(string name, string type, Guid? userId, bool deleted = false)
    {
        var file = File.Create(
            FileName.Create(name).Value,
            FileType.FromName(type),
            userId is { } owner ? UserId.Create(owner).Value : null).Value;
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        if (deleted)
        {
            file.BeginDeletion();
        }
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        repository.Add(file);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            repository.Delete(file);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return file;
    }
}
