using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

internal sealed class FileUploadFinalizer(
    FileManagerWriteDbContext dbContext,
    [FromKeyedServices(typeof(FileManagerWriteDbContext))] IUnitOfWork unitOfWork)
    : IFileUploadFinalizer
{
    public async Task<Result> CompleteAsync(
        FileId id,
        FileContentType contentType,
        FileSize size,
        CancellationToken cancellationToken)
    {
        var result = Result.Failure(Error.NotFound("File was not found."));
        await unitOfWork.ExecuteInTransactionAsync(
            async token =>
        {
            var file = await dbContext.Set<File>()
                .FromSqlInterpolated($"SELECT *, xmin FROM file_manager.files WHERE id = {id.Value} FOR UPDATE")
                .SingleOrDefaultAsync(token);
            if (file is null)
            {
                return;
            }

            await dbContext.Entry(file).ReloadAsync(token);
            result = file.CompleteUpload(contentType: contentType, size: size);
            if (result.IsSuccess)
            {
                await unitOfWork.SaveChangesAsync(token);
            }
        },
            cancellationToken);

        return result;
    }
}
