using Application.Common;
using Application.DTOs.TaskDtos;
using AutoMapper;
using Core.Domain;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository taskRepository, IMapper mapper, ICurrentUserProvider currentUserProvider, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _currentUserProvider = currentUserProvider;
            _logger = logger;
        }

        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching task with id {TaskId}", id);

            try
            {
                var item = await GetUserTaskOrThrowAsync(id);
                _logger.LogInformation("Task with id {TaskId} fetched successfully", id);

                return _mapper.Map<TaskDto>(item);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task with id {TaskId}", id);
                throw;
            }
        }
        public async Task<PaginatedResult<TaskDto>> GetAllTasksAsync(PaginationParams paginationParams)
        {
            _logger.LogInformation("Fetching tasks with pagination params {@PaginationParams}", paginationParams);
            try
            {
                var userId = _currentUserProvider.GetCurrentUserId();
                var (items, totalCount) = await _taskRepository.GetAllAsync(paginationParams, userId);

                _logger.LogInformation("Sucessfully fetched {TotalCount} tasks", totalCount);

                var paginatedResult = new PaginatedResult<TaskDto>(
                    _mapper.Map<IEnumerable<TaskDto>>(items),
                    totalCount,
                    paginationParams.PageSize,
                    paginationParams.PageNumber
                    );

                return paginatedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks with pagination params {@PaginationParams}", paginationParams);
                throw;
            }

        }
        public async Task<TaskDto> AddTaskAsync(CreateTaskDto task)
        {
            _logger.LogInformation("Adding new task with title {TaskTitle}", task.Title);
            try
            {
                var userId = _currentUserProvider.GetCurrentUserId();
                var newTask = _mapper.Map<TaskItem>(task);
                newTask.Id = Guid.NewGuid();
                newTask.UserId = Guid.Parse(userId);

                await _taskRepository.AddAsync(newTask);

                _logger.LogInformation("Sucessfully added a new task with title {TaskTitle}", newTask.Title);
                return _mapper.Map<TaskDto>(newTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task with title {TaskTitle}", task.Title);
                throw;
            }

        }
        public async Task UpdateTaskAsync(Guid id, UpdateTaskDto task)
        {

            _logger.LogInformation("Updating task with id {TaskId}", id);

            try
            {
                var item = await GetUserTaskOrThrowAsync(id);
                _mapper.Map(task, item);
                await _taskRepository.UpdateAsync(item);
                _logger.LogInformation("Sucessfully updated tasks with id {TaskId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with id {TaskId}", id);
                throw;
            }

        }
        public async Task DeleteTaskAsync(Guid id)
        {
            _logger.LogInformation("Deleting task with id {TaskId}", id);

            try
            {
                var item = await GetUserTaskOrThrowAsync(id);
                _logger.LogInformation("Sucessfully deleted task with id {TaskId}", id);

                await _taskRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with id {TaskId}", id);
                throw;
            }

        }
        public async Task CompleteTaskAsync(Guid id)
        {
            _logger.LogInformation("Completing task with id {TaskId}", id);

            try
            {
                var item = await GetUserTaskOrThrowAsync(id);
                if (item.IsCompleted) return;
                item.IsCompleted = true;
                item.CompletedAt = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(item);
                _logger.LogInformation("Sucessfully completed task with id {TaskId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task with id {TaskId}", id);
                throw;
            }

        }

        private async Task<TaskItem> GetUserTaskOrThrowAsync(Guid id)
        {
            var userId = _currentUserProvider.GetCurrentUserId();

            var item = await _taskRepository.GetByIdAsync(id);

            if (item is null)
            {
                _logger.LogWarning("Task with id {TaskId} not found for user {UserId}", id, userId);
                throw new NotFoundException($"Task with id {id} not found.");
            }

            if (item.UserId.ToString() != userId)
            {
                _logger.LogWarning("Task with id {TaskId} does not belong to the current user {UserId}", id, userId);
                throw new NotFoundException($"Task with id {id} does not belong to the current user.");
            }

            _logger.LogInformation("Task with id {TaskId} fetched successfully for user {UserId}", id, userId);
            return item;
        }
    }
}
