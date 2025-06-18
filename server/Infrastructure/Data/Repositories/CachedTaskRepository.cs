using Core.Domain;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Data.Repositories;

public class CachedTaskRepository : ITaskRepository
{
    private readonly TaskRepository _decorated;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CachedTaskRepository> _logger;

    public CachedTaskRepository(TaskRepository taskRepository, IDistributedCache distributedCache, ILogger<CachedTaskRepository> logger)
    {
        _distributedCache = distributedCache;
        _decorated = taskRepository;
        _logger = logger;
    }
    public Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        return _decorated.AddAsync(task, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _decorated.DeleteAsync(id, cancellationToken);

        var key = $"task-{id}";
        _logger.LogInformation("Invalidating cache for task ID {TaskId}", id);
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public Task<(IEnumerable<TaskItem> items, int totalCount)> GetAllAsync(PaginationParams paginationParams, string userId, CancellationToken cancellationToken)
    {
        return _decorated.GetAllAsync(paginationParams, userId, cancellationToken);
    }

    public async Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var key = $"task-{id.ToString()}";

        _logger.LogInformation("Attempting to retrieve task with ID {TaskId} from cache.", id);
        var task = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (task is null)
        {
            _logger.LogInformation("Cache miss for task ID {TaskId}. Retrieving from underlying source.", id);
            var item = await _decorated.GetByIdAsync(id, cancellationToken);

            _logger.LogInformation("Caching task with ID {TaskId}.", id);
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(item), cancellationToken);

            return item;
        }

        _logger.LogInformation("Cache hit for task ID {TaskId}. Returning cached item.", id);
        var deserialized = JsonConvert.DeserializeObject<TaskItem>(task)!;

        return deserialized;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await _decorated.UpdateAsync(task, cancellationToken);

        var key = $"task-{task.Id}";
        _logger.LogInformation("Invalidating cache for task ID {TaskId}", task.Id);
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
