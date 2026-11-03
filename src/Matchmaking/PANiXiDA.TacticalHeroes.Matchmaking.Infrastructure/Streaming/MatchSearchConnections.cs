using System.Collections.Concurrent;
using System.Threading.Channels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

internal sealed class MatchSearchConnections
{
    private readonly ConcurrentDictionary<Guid, Connection> _connections = new();

    public Connection Subscribe(Guid playerId)
    {
        var connection = new Connection(id: Guid.NewGuid(), playerId: playerId);
        _connections[connection.Id] = connection;
        return connection;
    }

    public void Remove(Connection connection)
    {
        if (_connections.TryRemove(connection.Id, out _))
        {
            connection.Changes.Writer.TryComplete();
        }
    }

    public void Refresh(Guid playerId)
    {
        foreach (var connection in _connections.Values.Where(connection => connection.PlayerId == playerId))
        {
            connection.Changes.Writer.TryWrite(item: true);
        }
    }

    public void RefreshAll()
    {
        foreach (var connection in _connections.Values)
        {
            connection.Changes.Writer.TryWrite(item: true);
        }
    }

    internal sealed class Connection(Guid id, Guid playerId)
    {
        public Guid Id { get; } = id;

        public Guid PlayerId { get; } = playerId;

        public Channel<bool> Changes { get; } = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }
}
