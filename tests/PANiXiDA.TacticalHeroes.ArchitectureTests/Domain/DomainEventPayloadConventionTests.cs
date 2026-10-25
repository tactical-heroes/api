using System.Collections.Immutable;

using PANiXiDA.Core.Domain.DomainEvents;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class DomainEventPayloadConventionTests
{
    [Fact(DisplayName = "Domain events should contain only primitive values when declared")]
    public void DomainEvents_Should_ContainOnlyPrimitiveValues_When_Declared()
    {
        var domainEvents = ArchitectureDefinition.s_productionAssemblies
            .Where(assembly => assembly.GetName().Name?.EndsWith(".Domain", StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(DomainEvent).IsAssignableFrom(type))
            .ToArray();
        var violations = domainEvents
            .SelectMany(DomainEventPayloadConvention.GetViolations)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(domainEvents);
        Assert.True(
            violations.Length == 0,
            $"Domain events must contain only primitive values:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Domain event payload should restrict member types when payload type varies")]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(char), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(DateTimeOffset), true)]
    [InlineData(typeof(DateOnly), true)]
    [InlineData(typeof(TimeOnly), true)]
    [InlineData(typeof(TimeSpan), true)]
    [InlineData(typeof(DayOfWeek), true)]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(Guid?), true)]
    [InlineData(typeof(DateTimeOffset?), true)]
    [InlineData(typeof(BattleId), false)]
    [InlineData(typeof(UnitId), false)]
    [InlineData(typeof(BattleId?), false)]
    [InlineData(typeof(Damage), false)]
    [InlineData(typeof(Unit), false)]
    [InlineData(typeof(BattleAggregate), false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(Uri), false)]
    [InlineData(typeof(ImmutablePayload), false)]
    [InlineData(typeof(int[]), false)]
    [InlineData(typeof(List<int>), false)]
    [InlineData(typeof(IReadOnlyCollection<int>), false)]
    [InlineData(typeof(ImmutableArray<int>), false)]
    [InlineData(typeof(ImmutableArray<Damage>), false)]
    public void DomainEventPayload_Should_RestrictMemberTypes_When_PayloadTypeVaries(Type payloadType, bool expected)
    {
        var eventType = typeof(PayloadEvent<>).MakeGenericType(payloadType);

        var violations = DomainEventPayloadConvention.GetViolations(eventType).ToArray();

        Assert.Equal(expected, violations.Length == 0);
    }

    [Theory(DisplayName = "Domain event payload should inspect hidden state when payload is not public")]
    [InlineData(typeof(FieldPayloadEvent))]
    [InlineData(typeof(PrivatePropertyPayloadEvent))]
    [InlineData(typeof(InheritedPayloadEvent))]
    public void DomainEventPayload_Should_InspectHiddenState_When_PayloadIsNotPublic(Type eventType)
    {
        var violations = DomainEventPayloadConvention.GetViolations(eventType).ToArray();

        Assert.NotEmpty(violations);
        Assert.All(violations, violation => Assert.Contains(nameof(Damage), violation));
    }

    private sealed record PayloadEvent<T>(T Value) : DomainEvent;

    private sealed record ImmutablePayload(int Value);

    private sealed record FieldPayloadEvent : DomainEvent
    {
        internal readonly Damage _result = null!;
    }

    private sealed record PrivatePropertyPayloadEvent : DomainEvent
    {
        private Damage Result { get; init; } = null!;
    }

    private abstract record PayloadEventBase : DomainEvent
    {
        public Damage Result { get; init; } = null!;
    }

    private sealed record InheritedPayloadEvent : PayloadEventBase;
}
