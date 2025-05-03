using Application.DTOs;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly IMapper _mapper;

        public TasksController(TaskService taskService, IMapper mapper)
        {
            _taskService = taskService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task is null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var task = await _taskService.AddTaskAsync(_mapper.Map<CreateTaskDto>(request));
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
        {
            await _taskService.UpdateTaskAsync(id, _mapper.Map<UpdateTaskDto>(request));
            return NoContent();
        }
    }
}
