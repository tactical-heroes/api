using System.Net.WebSockets;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

internal sealed class WatchMatchSearchEndpoint : IEndpoint<MatchSearchesEndpoints>
{
    public string Route { get; } = "/state";
    public string Name { get; } = "WatchMatchSearch";
    public string Summary { get; } = "Subscribe to current match search state over WebSocket";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapGet(builder.Route, HandleAsync)
            .Produces(StatusCodes.Status101SwitchingProtocols)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task HandleAsync(
        HttpContext context,
        IMediator mediator,
        IOptions<JsonOptions> jsonOptions,
        CancellationToken cancellationToken)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await TypedResults.Problem(
                detail: "A WebSocket connection is required.",
                statusCode: StatusCodes.Status400BadRequest)
                .ExecuteAsync(context);
            return;
        }

        var result = await mediator.SendAsync(WatchMatchSearchMapper.ToCommand(), cancellationToken);
        if (result.IsFailure)
        {
            await result.ToHttpResult(onSuccess: _ => TypedResults.NoContent()).ExecuteAsync(context);
            return;
        }

        await using var subscription = result.Value;
        var protocol = context.WebSockets.WebSocketRequestedProtocols.Contains(MatchSearchWebSocketExtensions.Protocol)
            ? MatchSearchWebSocketExtensions.Protocol : null;
        using var socket = await context.WebSockets.AcceptWebSocketAsync(subProtocol: protocol);
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await SendAsync(
                socket: socket,
                state: subscription.Current,
                jsonOptions: jsonOptions.Value.SerializerOptions,
                cancellationToken: lifetime.Token);
            var sending = SendChangesAsync(
                socket: socket,
                subscription: subscription,
                jsonOptions: jsonOptions.Value.SerializerOptions,
                cancellationToken: lifetime.Token);
            var receiving = ReceiveAsync(socket, lifetime.Token);
            await Task.WhenAny(sending, receiving);
            await lifetime.CancelAsync();
            try
            {
                await Task.WhenAll(sending, receiving);
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
            {
                // Stop the sender before writing a close frame.
            }

            if (receiving.IsCompletedSuccessfully)
            {
                var status = await receiving;
                await socket.CloseOutputAsync(
                    closeStatus: status,
                    statusDescription: "This connection only receives match search state.",
                    cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            // The peer disconnected or the HTTP request was aborted.
        }
        catch (WebSocketException)
        {
            // Abrupt disconnects are normal for browser clients.
        }
        finally
        {
            await lifetime.CancelAsync();
        }
    }

    private static async Task SendChangesAsync(
        WebSocket socket,
        IMatchSearchStateSubscription subscription,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken)
    {
        await foreach (var state in subscription.ReadChangesAsync(cancellationToken))
        {
            await SendAsync(
                socket: socket, state: state, jsonOptions: jsonOptions, cancellationToken: cancellationToken);
        }
    }

    private static ValueTask SendAsync(
        WebSocket socket,
        MatchSearchStateReadModel state,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> message = JsonSerializer.SerializeToUtf8Bytes(state, jsonOptions);
        return socket.SendAsync(
            buffer: message,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: cancellationToken);
    }

    private static async Task<WebSocketCloseStatus> ReceiveAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        var message = await socket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
        return message.MessageType == WebSocketMessageType.Close
            ? WebSocketCloseStatus.NormalClosure
            : WebSocketCloseStatus.PolicyViolation;
    }
}
