using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Domain;
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
        public async Task<TaskItem> AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(_mapper.Map<TaskEntity>(task));
            await _context.SaveChangesAsync();

            return task;
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

        public async Task<(IEnumerable<TaskItem>, int)> GetAllAsync(PaginationParams paginationParams)
        {

            var query = _context.Tasks
                .AsNoTracking()
                .Where(task => task.Title.Contains(paginationParams.SearchTerm));

            var totalCount = await query.CountAsync();

            return (await query
                .OrderBy(task => task.CreatedAt)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ProjectTo<TaskItem>(_mapper.ConfigurationProvider)
                .ToListAsync(), totalCount);
        }

        public async Task<TaskItem> GetByIdAsync(Guid id)
        {
            var item = await _context.Tasks
                .AsNoTracking()
                .ProjectTo<TaskItem>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(task => task.Id == id);

            if (item is null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }
            return item;
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(_mapper.Map<TaskEntity>(task));
            await _context.SaveChangesAsync();
        }
    }
}
