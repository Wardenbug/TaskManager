using Application.Common;
using Application.DTOs.TaskDtos;
using AutoMapper;
using Core.Domain;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;

namespace Application.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserProvider _currentUserProvider;

        public TaskService(ITaskRepository taskRepository, IMapper mapper, ICurrentUserProvider currentUserProvider)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            var item = await GetUserTaskOrThrowAsync(id);

            return _mapper.Map<TaskDto>(item);
        }
        public async Task<PaginatedResult<TaskDto>> GetAllTasksAsync(PaginationParams paginationParams)
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            var (items, totalCount) = await _taskRepository.GetAllAsync(paginationParams, userId);

            var paginatedResult = new PaginatedResult<TaskDto>(
                _mapper.Map<IEnumerable<TaskDto>>(items),
                totalCount,
                paginationParams.PageSize,
                paginationParams.PageNumber
                );

            return paginatedResult;
        }
        public async Task<TaskDto> AddTaskAsync(CreateTaskDto task)
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            var newTask = _mapper.Map<TaskItem>(task);
            newTask.Id = Guid.NewGuid();
            newTask.UserId = Guid.Parse(userId);

            await _taskRepository.AddAsync(newTask);
            return _mapper.Map<TaskDto>(newTask);
        }
        public async Task UpdateTaskAsync(Guid id, UpdateTaskDto task)
        {
            var item = await GetUserTaskOrThrowAsync(id);

            _mapper.Map(task, item);

            await _taskRepository.UpdateAsync(item);
        }
        public async Task DeleteTaskAsync(Guid id)
        {
            var item = await GetUserTaskOrThrowAsync(id);

            await _taskRepository.DeleteAsync(id);
        }
        public async Task CompleteTaskAsync(Guid id)
        {
            var item = await GetUserTaskOrThrowAsync(id);
            if (item.IsCompleted) return;
            item.IsCompleted = true;
            item.CompletedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(item);
        }

        private async Task<TaskItem> GetUserTaskOrThrowAsync(Guid id)
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            var item = await _taskRepository.GetByIdAsync(id);

            if (item is null || item.UserId.ToString() != userId)
            {
                throw new NotFoundException($"Task with id {id} not found.");
            }

            return item;
        }
    }
}
