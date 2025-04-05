namespace WebApi.Extensions;

public static class HttpRequestExtensions
{
    public static Dictionary<string, string> GetFilters(this HttpRequest request)
    {
        return request.Query.ToDictionary(x => ToPascalCase(x.Key), x => x.Value.ToString());
    }

    private static string ToPascalCase(string input)
    {
        return char.ToUpper(input[0]) + input[1..];
    }
}
