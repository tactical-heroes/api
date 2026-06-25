using Organization.Product.Module.Application.Users.GetDetails;

using Riok.Mapperly.Abstractions;

namespace Organization.Product.Module.Presentation.Features.Users.GetDetails;

[Mapper]
internal static partial class UserDetailsMapper
{
    internal static partial UserDetailsResponse ToResponse(UserDetailsReadModel dto);
}
