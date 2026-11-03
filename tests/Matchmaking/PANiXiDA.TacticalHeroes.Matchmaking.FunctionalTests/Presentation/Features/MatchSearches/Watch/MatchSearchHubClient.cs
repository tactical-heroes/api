using System.Threading.Channels;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Watch;

internal sealed class MatchSearchHubClient(HubConnection connection, ChannelReader<MatchSearchStateResponse> messages)
    : IAsyncDisposable
{
    private long _version = -1;

    public ValueTask DisposeAsync() => connection.DisposeAsync();

    internal static async Task<MatchSearchHubClient> ConnectAsync(
        WebApplicationFactory<Program> factory, Guid userId, CancellationToken cancellationToken)
    {
        var messages = Channel.CreateUnbounded<MatchSearchStateResponse>();
        var connection = new HubConnectionBuilder()
            .WithUrl(
                "http://localhost/api/v1/match-searches/state",
                options =>
            {
                options.Headers["X-Test-UserId"] = userId.ToString();
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.WebSocketFactory = (context, token) =>
                {
                    var client = factory.Server.CreateWebSocketClient();
                    client.ConfigureRequest = request => request.Headers["X-Test-UserId"] = userId.ToString();
                    return new ValueTask<System.Net.WebSockets.WebSocket>(client.ConnectAsync(context.Uri, token));
                };
            })
            .Build();
        connection.On<MatchSearchStateResponse>("StateChanged", state => { messages.Writer.TryWrite(state); });
        try
        {
            await connection.StartAsync(cancellationToken);
            return new MatchSearchHubClient(connection, messages.Reader);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    internal async Task<MatchSearchStateResponse> ReadAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var state = await messages.ReadAsync(cancellationToken);
            if (state.Version > _version)
            {
                _version = state.Version;
                return state;
            }
        }
    }
}
