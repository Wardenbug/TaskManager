using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> GetByIdAsync(Guid id);
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(Guid id);
    }
}
