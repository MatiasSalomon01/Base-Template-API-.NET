using Domain.Abstractions.Common;
using Domain.Abstractions.Interfaces;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public virtual DbSet<Product> Products { get; set; }


    private static readonly List<(Type, string)> InterfacesConfig = 
    [
        (typeof(ISoftDelete), nameof(ApplySoftDelete))
    ];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyAbstractionConfiguration(modelBuilder, InterfacesConfig);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyAbstractionConfiguration(ModelBuilder modelBuilder, List<(Type, string)> configurations)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            foreach (var (interfaceType, methodName) in configurations)
            {
                if (interfaceType.IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)?
                        .MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(null, [modelBuilder]);
                }
            }
        }
    }

    private static void ApplySoftDelete<T>(ModelBuilder modelBuilder) where T : Entity, ISoftDelete
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<T>().Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
