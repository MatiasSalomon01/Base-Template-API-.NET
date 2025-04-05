namespace WebApi.Feature.Products.Responses;

public record ProductQueryResult(int Id, string Name, string? Description, decimal Price, int Stock, DateTime CreatedAt);
