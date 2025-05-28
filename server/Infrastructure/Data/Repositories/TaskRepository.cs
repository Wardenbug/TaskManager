using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Domain;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskRepository> _logger;

        public TaskRepository(AppDbContext context, IMapper mapper, ILogger<TaskRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding a new task with title: {Title} to DB", task.Title);
            try
            {
                await _context.Tasks.AddAsync(_mapper.Map<TaskEntity>(task), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Successfully added task with ID: {Id} to DB", task.Id);
                return task;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task to DB");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting task with id {Id} from DB", id);
            try
            {
                var task = await _context.Tasks.FindAsync(id, cancellationToken);
                if (task is not null)
                {
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug("Successfully deleted task with id {Id} from DB", id);
                }
                else
                {
                    _logger.LogWarning("Attempted to delete task with id {Id}, but it was not found in DB", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with id {Id} in DB", id);
                throw;
            }
        }

        public async Task<(IEnumerable<TaskItem>, int)> GetAllAsync(PaginationParams paginationParams, string userId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Fetching tasks for user {UserId} with pagination params {@PaginationParams}", userId, paginationParams);
            try
            {
                var query = _context.Tasks
                   .AsNoTracking()
                   .Where(task => task.UserId.ToString() == userId)
                   .Where(task => task.Title.Contains(paginationParams.SearchTerm));

                var totalCount = await query.CountAsync(cancellationToken);

                var tasks = await query
                    .OrderBy(task => task.CreatedAt)
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ProjectTo<TaskItem>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("Sucessfully fetched {TotalCount} tasks for user {UserId}", totalCount, userId);

                return (tasks, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks for user {UserId} with pagination params {@PaginationParams}", userId, paginationParams);
                throw;
            }

        }
        public async Task<TaskItem> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching task with id {Id}", id);
            try
            {
                var item = await _context.Tasks
                    .AsNoTracking()
                    .ProjectTo<TaskItem>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

                if (item is null)
                {
                    _logger.LogWarning("Task with id {Id} not found", id);
                    throw new NotFoundException($"Task with id {id} not found");
                }
                _logger.LogDebug("Successfully fetched task with id {Id}", id);
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task with id {Id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating task with id {Id}", task.Id);
            try
            {
                _context.Tasks.Update(_mapper.Map<TaskEntity>(task));
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Sucessfuly updated task with id {Id}", task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with id {Id}", task.Id);
                throw;
            }

        }
    }
}
