using System.Threading.Channels;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Watch;

internal sealed class MatchSearchHubClient(HubConnection connection, ChannelReader<MatchSearchStateResponse> messages)
    : IAsyncDisposable
{
    public ValueTask DisposeAsync() => connection.DisposeAsync();

    internal static async Task<MatchSearchHubClient> ConnectAsync(
        WebApplicationFactory<Program> factory,
        Guid userId,
        bool messagePack,
        CancellationToken cancellationToken)
    {
        var messages = Channel.CreateUnbounded<MatchSearchStateResponse>();
        var builder = new HubConnectionBuilder()
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
            });
        if (messagePack) { builder.AddMessagePackProtocol(); }
        var connection = builder.Build();
        connection.On<MatchSearchStateResponse>("StateChangedAsync", state => { messages.Writer.TryWrite(state); });
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

    internal ValueTask<MatchSearchStateResponse> ReadAsync(CancellationToken cancellationToken)
    {
        return messages.ReadAsync(cancellationToken);
    }
}
