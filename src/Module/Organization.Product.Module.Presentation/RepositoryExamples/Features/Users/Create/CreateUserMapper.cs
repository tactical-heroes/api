using Organization.Product.Module.Application.Users.Create;

using Riok.Mapperly.Abstractions;

namespace Organization.Product.Module.Presentation.Features.Users.Create;

[Mapper]
internal static partial class CreateUserMapper
{
    internal static partial CreateUserCommand ToCommand(CreateUserRequest request);
    internal static partial CreateUserResponse ToResponse(Guid id);
}
