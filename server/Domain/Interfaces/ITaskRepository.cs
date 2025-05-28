using Core.Domain;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<(IEnumerable<TaskItem> items, int totalCount)> GetAllAsync(PaginationParams paginationParams, string userId, CancellationToken cancellationToken);
        Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken);
        Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
