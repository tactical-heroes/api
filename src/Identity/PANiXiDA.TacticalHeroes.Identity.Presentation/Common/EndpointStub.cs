using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

internal static class EndpointStub
{
    public static IResult NotImplemented(string operationName)
    {
        return TypedResults.Problem(
            title: "Application use case is not implemented.",
            detail: $"The '{operationName}' HTTP contract is available, but its application use case will be added in the next stage.",
            statusCode: StatusCodes.Status501NotImplemented);
    }
}
