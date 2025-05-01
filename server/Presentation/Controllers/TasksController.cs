using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;

        public TasksController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "New Task",
                Description = "Task Description",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null
            };
            await _taskRepository.AddAsync(taskItem);

            return Ok(taskItem);
        }
    }
}
