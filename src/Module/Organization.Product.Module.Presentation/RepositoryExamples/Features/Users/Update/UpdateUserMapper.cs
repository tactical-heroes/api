using Organization.Product.Module.Application.Users.Update;

using Riok.Mapperly.Abstractions;

namespace Organization.Product.Module.Presentation.Features.Users.Update;

[Mapper]
internal static partial class UpdateUserMapper
{
    internal static partial UpdateUserCommand ToCommand(UpdateUserRequest request, Guid id);
}
