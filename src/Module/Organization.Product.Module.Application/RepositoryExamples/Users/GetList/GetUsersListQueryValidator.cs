namespace Organization.Product.Module.Application.Users.GetList;

public sealed class GetUsersListQueryValidator : AbstractValidator<GetUsersListQuery>
{
    public GetUsersListQueryValidator()
    {
        RuleFor(query => query.FilterParameters)
            .NotNull();

        RuleFor(query => query.PaginationParameters)
            .NotNull();

        When(query => query.PaginationParameters is not null, () =>
        {
            RuleFor(query => query.PaginationParameters.PageNumber)
                .GreaterThan(0);

            RuleFor(query => query.PaginationParameters.PageSize)
                .GreaterThan(0);
        });

        RuleFor(query => query.SortParameters)
            .NotNull();
    }
}
