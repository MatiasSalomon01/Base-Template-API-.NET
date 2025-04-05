namespace WebApi.Feature.Products.Requests;

public class UpdateProductRequest : CreateProductRequest
{
    public int Id { get; set; }
}