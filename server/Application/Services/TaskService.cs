using Application.DTOs;
using AutoMapper;
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
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var items = await _taskRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TaskDto>>(items);
        }
        public async Task<TaskDto> AddTaskAsync(CreateTaskDto task)
        {
            var newTask = _mapper.Map<TaskItem>(task);
            newTask.Id = Guid.NewGuid();

            await _taskRepository.AddAsync(newTask);
            return _mapper.Map<TaskDto>(newTask);
        }
        public async Task UpdateTaskAsync(Guid id, UpdateTaskDto task)
        {
            var item = await _taskRepository.GetByIdAsync(id);

            if(item is null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found.");
            }

            var testItem = _mapper.Map(task, item);

            await _taskRepository.UpdateAsync(testItem);

        }
        public async Task DeleteTaskAsync(Guid id)
        {
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
