using Domain.Abstractions.Common;
using Domain.Abstractions.Interfaces;
using Intrastructure.Abstractions.Extensions;
using Intrastructure.Abstractions.Interfaces.Pagination;
using Intrastructure.Abstractions.Interfaces.Repositories;
using Intrastructure.Abstractions.Models.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class EntityRepository(DbContext context) : IEntityRepository
{
    public async Task<T?> GetById<T>(int id, bool withTracking = false) where T : Entity
    {
        var query = withTracking 
            ? context.Set<T>().AsQueryable()
            : context.Set<T>().AsNoTracking();

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<M>> GetPaginated<T, M>(IPaginatedOptions options) where T : Entity
    {
        var queryable = context.Set<T>()
            .AsNoTracking()
            .ApplyGenericFilter(options.Filters)
            .GeneralSearch(options.Search);

        var count = await queryable.CountAsync();

        var models = await queryable
            .ApplySort(options.SortBy, options.Direction)
            .ApplyPagination(options)
            .ProjectToType<M>()
            .ToListAsync();

        bool hasNext = ((options.PageNumber + 1) * options.PageSize) < count;
        bool hasPrevious = options.PageNumber > 1;

        return models.ToPaginated(count, options, hasNext, hasPrevious);
    }

    public async Task<int> Create<T>(T entity) where T : Entity
    {
        await context.Set<T>().AddAsync(entity);
        return await context.SaveChangesAsync();
    }

    public async Task Update<T>(T entity, bool hasTracking = false) where T : Entity
    {
        if (hasTracking)
        {
            context.Set<T>().Update(entity);
        }

        await context.SaveChangesAsync();
    }

    public async Task Delete<T>(int id) where T : Entity, ISoftDelete
    {
        var now = DateTime.Now;

        await context.Set<T>()
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.IsDeleted, true)
                                      .SetProperty(x => x.DeletedAt, now));
    }

    public async Task DeleteRange<T>(int[] ids) where T : Entity, ISoftDelete
    {
        var now = DateTime.Now;

        await context.Set<T>()
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.IsDeleted, true)
                                      .SetProperty(x => x.DeletedAt, now));
    }
}
