using Core.Domain;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> GetByIdAsync(Guid id);
        Task<(IEnumerable<TaskItem> items, int totalCount)> GetAllAsync(PaginationParams paginationParams);
        Task<TaskItem> AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(Guid id);
    }
}
