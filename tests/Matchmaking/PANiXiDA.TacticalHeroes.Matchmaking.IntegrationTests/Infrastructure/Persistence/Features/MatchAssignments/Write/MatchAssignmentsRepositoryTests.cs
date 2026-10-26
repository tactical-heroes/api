using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchAssignments.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchAssignmentsRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Matchmaking should preserve confirmations and game when reloaded between transitions")]
    public async Task GetByIdAsync_Should_PreserveConfirmationsAndGame_When_ReloadedBetweenTransitions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.MatchAssignments.MatchAssignment proposal;
        await using (var scope = fixture.CreateScope())
        {
            proposal = await SaveProposalAsync(scope.ServiceProvider, cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
            var restored = (await repository.GetByIdAsync(proposal.Id, cancellationToken))!;
            restored.Participants.Count.ShouldBe(2);
            restored.Accept(restored.Participants.First().Id, At(2)).IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        var game = GameReference.Create(Guid.CreateVersion7()).Value;
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
            var restored = (await repository.GetByIdAsync(proposal.Id, cancellationToken))!;
            restored.Participants.Count(value => value.AcceptedAt is not null).ShouldBe(1);
            restored.Accept(restored.Participants.Single(value => value.AcceptedAt is null).Id, At(3))
                .IsSuccess
                .ShouldBeTrue();
            restored.Complete(game, At(4)).IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var row = await readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchAssignmentReadDbModel>()
            .Include(value => value.Participants)
            .SingleAsync(value => value.Id == proposal.Id.Value, cancellationToken);
        row.GameId.ShouldBe(game.Value);
        row.Status.ShouldBe(MatchAssignmentStatus.Ready.Name);
        row.Participants.Count.ShouldBe(2);
        row.Participants.ShouldAllBe(value => value.AcceptedAt != null);
    }

    [Fact(DisplayName = "Matchmaking should reject stale confirmation when participants confirm concurrently")]
    public async Task Update_Should_RejectStaleConfirmation_When_ParticipantsConfirmConcurrently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.MatchAssignments.MatchAssignment proposal;
        await using (var scope = fixture.CreateScope())
        {
            proposal = await SaveProposalAsync(scope.ServiceProvider, cancellationToken);
        }

        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var first = (await firstRepository.GetByIdAsync(proposal.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(proposal.Id, cancellationToken))!;
        var firstParticipant = first.Participants.First().Id;
        first.Accept(firstParticipant, At(2)).IsSuccess.ShouldBeTrue();
        firstRepository.Update(first);
        await UnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.Accept(second.Participants.Single(value => value.Id != firstParticipant).Id, At(2))
            .IsSuccess
            .ShouldBeTrue();
        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var retryScope = fixture.CreateScope();
        var retryRepository = retryScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var retried = (await retryRepository.GetByIdAsync(proposal.Id, cancellationToken))!;
        retried.Participants.Count(value => value.AcceptedAt is not null).ShouldBe(1);
        retried.Accept(retried.Participants.Single(value => value.AcceptedAt is null).Id, At(3))
            .IsSuccess
            .ShouldBeTrue();
        retryRepository.Update(retried);
        await UnitOfWork(retryScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        retried.Status.ShouldBe(MatchAssignmentStatus.CreatingGame);
    }
}
