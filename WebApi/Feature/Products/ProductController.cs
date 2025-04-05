using Application.Exceptions;
using Domain.Products;
using Intrastructure.Abstractions.Interfaces.Repositories;
using Intrastructure.Abstractions.Models.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.Feature.Products.Requests;
using WebApi.Feature.Products.Responses;

namespace WebApi.Feature.Products;

public sealed class ProductController(IEntityRepository repository) : BaseController
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await repository.GetById<Product>(id) ?? throw new NotFoundException();

        var model = entity.Adapt<ProductQueryResult>();

        return Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaginated([FromQuery] PaginatedOptions options)
    {
        var entities = await repository.GetPaginated<Product, ProductQueryResult>(options);
        return Ok(entities);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var entity = Product.Create(request.Name, request.Description, request.Price, request.Stock);

        var id = await repository.Create(entity);

        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await repository.GetById<Product>(id, withTracking: true) ?? throw new NotFoundException();

        product.Update(request.Name, request.Description, request.Price, request.Stock);

        await repository.Update(product);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.Delete<Product>(id);

        return NoContent();
    }
}
