using System.Text.Json;

namespace Organization.Product.Testing.Json;

public static class TestJsonSerializerOptions
{
    public static JsonSerializerOptions Web { get; } = new(JsonSerializerOptions.Web)
    {
        Converters = { new PaginationResultJsonConverterFactory() }
    };
}
