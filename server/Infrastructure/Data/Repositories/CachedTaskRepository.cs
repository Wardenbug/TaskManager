using Core.Domain;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Infrastructure.Data.Repositories;

public class CachedTaskRepository : ITaskRepository
{
    private readonly TaskRepository _decorated;
    private readonly IDistributedCache _distributedCache;

    public CachedTaskRepository(TaskRepository taskRepository, IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _decorated = taskRepository;
    }
    public Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        return _decorated.AddAsync(task, cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return _decorated.DeleteAsync(id, cancellationToken);
    }

    public Task<(IEnumerable<TaskItem> items, int totalCount)> GetAllAsync(PaginationParams paginationParams, string userId, CancellationToken cancellationToken)
    {
       return _decorated.GetAllAsync(paginationParams, userId, cancellationToken);
    }

    public async Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var key = $"task-{id.ToString()}";

        var task = await _distributedCache.GetStringAsync(key, cancellationToken);

        if(task is null)
        {
            var item = await _decorated.GetByIdAsync(id, cancellationToken);

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(item));
        }

        return await _decorated.GetByIdAsync(id, cancellationToken);
    }

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        return _decorated.UpdateAsync(task, cancellationToken);
    }
}
