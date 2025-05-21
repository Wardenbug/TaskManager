using Application.Common;
using Application.DTOs.TaskDtos;
using AutoMapper;
using Core.Domain;
using Core.Domain.Entities;
using Core.Interfaces;

namespace Application.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }
        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            var item = await _taskRepository.GetByIdAsync(id);

            if (item is null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found.");
            }

            return _mapper.Map<TaskDto>(item);
        }
        public async Task<PaginatedResult<TaskDto>> GetAllTasksAsync(PaginationParams paginationParams)
        {
            var (items, totalCount) = await _taskRepository.GetAllAsync(paginationParams);

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
            var newTask = _mapper.Map<TaskItem>(task);
            newTask.Id = Guid.NewGuid();
            newTask.UserId = Guid.Parse(task.UserId);

            await _taskRepository.AddAsync(newTask);
            return _mapper.Map<TaskDto>(newTask);
        }
        public async Task UpdateTaskAsync(Guid id, UpdateTaskDto task)
        {
            var item = await _taskRepository.GetByIdAsync(id);

            if (item is null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found.");
            }

            _mapper.Map(task, item);

            await _taskRepository.UpdateAsync(item);

        }
        public async Task DeleteTaskAsync(Guid id)
        {
            var item = await _taskRepository.GetByIdAsync(id);
            if (item is null)
                throw new KeyNotFoundException($"Task with id {id} not found.");

            await _taskRepository.DeleteAsync(id);
        }
        public async Task CompleteTaskAsync(Guid id)
        {
            var item = await _taskRepository.GetByIdAsync(id);
            if (item is not null)
            {
                item.IsCompleted = true;
                item.CompletedAt = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(item);
            }
        }
    }
}
