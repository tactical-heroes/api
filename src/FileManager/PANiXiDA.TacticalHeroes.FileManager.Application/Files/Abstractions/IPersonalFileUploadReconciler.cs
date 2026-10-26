namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IPersonalFileUploadReconciler
{
    Task ReconcileAsync(CancellationToken cancellationToken);
}
