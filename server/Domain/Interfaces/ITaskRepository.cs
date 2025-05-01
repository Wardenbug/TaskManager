using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> GetByIdAsync(int id);
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(int id);
    }
}
