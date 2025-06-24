using Application.Common;
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

        /// <summary>
        /// Returns a paginated list of tasks.
        /// </summary>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of tasks.</returns>
        [ProducesResponseType(typeof(PaginatedResult<TaskDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all tasks with pagination parameters: {@PaginationParams}", paginationParams);
            try
            {
                var tasks = await _taskService.GetAllTasksAsync(paginationParams, cancellationToken);

                _logger.LogInformation("Successfully retrieved {TaskCount} tasks.", tasks.Items.Count());
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching tasks. PaginationsParams: {paginationParams}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete task with ID {TaskId}", id);
            try
            {
                await _taskService.DeleteTaskAsync(id, cancellationToken);
                _logger.LogInformation("Successfully deleted task with ID {TaskId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                throw;
            }
        }

        /// <summary>
        /// Returns a task by its ID.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task details.</returns>
        [ProducesResponseType(typeof(TaskDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching task with ID {TaskId}", id);
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
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

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="request">Task creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created task.</returns>
        [ProducesResponseType(typeof(TaskDto), 201)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new task with title: {TaskTitle}", request.Title);
            try
            {
                var createTaskDto = _mapper.Map<CreateTaskDto>(request);
                var task = await _taskService.AddTaskAsync(createTaskDto, cancellationToken);
                _logger.LogInformation("Successfully created task with ID {TaskId}", task.Id);
                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw;
            }

        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <param name="request">Task update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating task with id: {Id}", id);
            try
            {
                await _taskService.UpdateTaskAsync(id, _mapper.Map<UpdateTaskDto>(request), cancellationToken);
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
