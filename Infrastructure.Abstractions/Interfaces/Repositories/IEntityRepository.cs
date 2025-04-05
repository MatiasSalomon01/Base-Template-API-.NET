using Domain.Abstractions.Common;
using Domain.Abstractions.Interfaces;
using Intrastructure.Abstractions.Interfaces.Pagination;
using Intrastructure.Abstractions.Models.Pagination;

namespace Intrastructure.Abstractions.Interfaces.Repositories;

public interface IEntityRepository
{
    Task<T?> GetById<T>(int id, bool withTracking = false) where T : Entity;
    Task<PaginatedResult<M>> GetPaginated<T, M>(IPaginatedOptions options) where T : Entity;
    Task<int> Create<T>(T entity) where T : Entity;
    Task Update<T>(T entity, bool hasTracking = false) where T : Entity;
    Task Delete<T>(int id) where T : Entity, ISoftDelete;
}