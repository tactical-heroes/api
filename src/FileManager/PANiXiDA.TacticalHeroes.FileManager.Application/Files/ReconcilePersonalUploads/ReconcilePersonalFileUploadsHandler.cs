using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandler(IPersonalFileUploadReconciler reconciler)
    : ICommandHandler<ReconcilePersonalFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcilePersonalFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        await reconciler.ReconcileAsync(cancellationToken);

        return Result.Success();
    }
}
