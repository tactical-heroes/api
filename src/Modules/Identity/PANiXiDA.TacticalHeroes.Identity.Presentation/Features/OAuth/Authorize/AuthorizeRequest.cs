using Microsoft.AspNetCore.Mvc;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

public sealed record AuthorizeRequest(
    [property: FromQuery(Name = "client_id")] string ClientId,
    [property: FromQuery(Name = "request_uri")] string RequestUri);
