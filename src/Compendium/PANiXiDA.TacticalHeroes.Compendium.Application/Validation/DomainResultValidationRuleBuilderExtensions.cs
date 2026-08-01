namespace PANiXiDA.TacticalHeroes.Compendium.Application.Validation;

internal static class DomainResultValidationRuleBuilderExtensions
{
    public static IRuleBuilderOptionsConditions<TRequest, TRequest>
        MustBeValidDomainResult<TRequest, TValue>(
            this IRuleBuilder<TRequest, TRequest> ruleBuilder,
            Func<TRequest, Result<TValue>> factory)
    {
        return ruleBuilder.Custom((request, context) =>
        {
            var result = factory(request);

            foreach (var error in result.Errors)
            {
                var propertyName = error.Metadata.TryGetValue(
                        Error.FieldMetadataKey,
                        out var field) &&
                    field is string fieldName &&
                    !string.IsNullOrWhiteSpace(fieldName)
                        ? fieldName
                        : context.PropertyPath;

                context.AddFailure(propertyName, error.Message);
            }
        });
    }
}
