using AutoMapper;
using Core.Domain.Entities;
using Core.Interfaces;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TaskRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(_mapper.Map<TaskEntity>(task));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task is not null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Select(t => new TaskItem
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt
                })
                .ToListAsync();
        }

        public async Task<TaskItem> GetByIdAsync(Guid id)
        {
            var item = await _context.Tasks.FindAsync(id);
            if (item is null)
            {
                throw new Exception($"Task with id {id} not found");
            }
            return new TaskItem
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                IsCompleted = item.IsCompleted,
                CreatedAt = item.CreatedAt,
                CompletedAt = item.CompletedAt
            };
        }

        public Task UpdateAsync(TaskItem task)
        {
            throw new NotImplementedException();
        }
    }
}
