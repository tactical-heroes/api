using NSubstitute.ExceptionExtensions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandlerTests
{
    [Fact(DisplayName = "Reconciliation command should invoke the reconciler when command is handled")]
    public async Task HandleAsync_Should_InvokeReconciler_When_CommandIsHandled()
    {
        var reconciler = Substitute.For<IPersonalFileUploadReconciler>();
        var handler = new ReconcilePersonalFileUploadsHandler(reconciler);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new ReconcilePersonalFileUploadsCommand(), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await reconciler.Received(1).ReconcileAsync(cancellationToken);
    }

    [Fact(DisplayName = "Reconciliation command should propagate failure when reconciliation fails")]
    public async Task HandleAsync_Should_PropagateFailure_When_ReconciliationFails()
    {
        var reconciler = Substitute.For<IPersonalFileUploadReconciler>();
        var failure = new IOException("Database is unavailable.");
        reconciler.ReconcileAsync(Arg.Any<CancellationToken>()).ThrowsAsync(failure);
        var handler = new ReconcilePersonalFileUploadsHandler(reconciler);

        var thrown = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken));

        thrown.ShouldBeSameAs(failure);
    }
}
