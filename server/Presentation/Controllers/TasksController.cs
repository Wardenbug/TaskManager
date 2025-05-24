using Application.DTOs.TaskDtos;
using Application.Services;
using AutoMapper;
using Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly IMapper _mapper;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskService taskService, IMapper mapper, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
        {
            _logger.LogInformation("Fetching all tasks with pagination parameters: {@PaginationParams}", paginationParams);
            try
            {
                var tasks = await _taskService.GetAllTasksAsync(paginationParams);

                _logger.LogInformation("Successfully retrieved {TaskCount} tasks.", tasks.Items.Count());
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching tasks. PaginationsParams: {paginationParams}");
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Attempting to delete task with ID {TaskId}", id);
            try
            {
                await _taskService.DeleteTaskAsync(id);
                _logger.LogInformation("Successfully deleted task with ID {TaskId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Fetching task with ID {TaskId}", id);
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task is null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found", id);
                    return NotFound();
                }
                _logger.LogInformation("Successfully retrieved task with ID {TaskId}", id);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task with ID {TaskId}", id);
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            _logger.LogInformation("Creating a new task with title: {TaskTitle}", request.Title);
            try
            {
                var createTaskDto = _mapper.Map<CreateTaskDto>(request);
                var task = await _taskService.AddTaskAsync(createTaskDto);
                _logger.LogInformation("Successfully created task with ID {TaskId}", task.Id);
                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw;
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
        {
            _logger.LogInformation("Updating task with id: {Id}", id);
            try
            {
                await _taskService.UpdateTaskAsync(id, _mapper.Map<UpdateTaskDto>(request));
                _logger.LogInformation("Successfully updated task with ID {TaskId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID {TaskId}", id);
                throw;
            }
        }
    }
}
