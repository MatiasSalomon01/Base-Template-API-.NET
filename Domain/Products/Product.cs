using Domain.Abstractions.Common;
using Domain.Abstractions.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Domain.Products;

public class Product : Entity, IAuditTime, ISoftDelete, IGeneralSearchBy
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public static List<string>? SearchByProperties => [nameof(Name), nameof(Description), nameof(Price), nameof(Stock), nameof(CreatedAt)];

    private Product(){ }

    public static Product Create(string name, string? description, decimal price, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        CustomArgumentException.ThrowIfLessThanZero<decimal>(price, nameof(price));
        CustomArgumentException.ThrowIfLessThanZero<int>(stock, nameof(stock));

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock
        };
    }

    public void Update(string name, string? description, decimal price, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        CustomArgumentException.ThrowIfLessThanZero<decimal>(price, nameof(price));
        CustomArgumentException.ThrowIfLessThanZero<int>(stock, nameof(stock));

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }
}

public class CustomArgumentException : ArgumentException
{
    public static void ThrowIfLessThanZero<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? paramName = null) where T : struct, INumber<T>
    {
        if (argument is null || argument.Value < T.Zero)
        {
            throw new ArgumentException("El valor no puede ser menor que cero.", paramName);
        }
    }
}