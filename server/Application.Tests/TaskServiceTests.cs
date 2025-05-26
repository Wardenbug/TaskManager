namespace Application.Tests;

using Application.DTOs.TaskDtos;
using Application.Services;
using AutoMapper;
using Core.Domain;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class TaskServiceTests
{

    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
        _mockLogger = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(
            _mockTaskRepository.Object,
            _mockMapper.Object,
            _mockCurrentUserProvider.Object,
            _mockLogger.Object
        );
    }


    // --- GetTaskByIdAsync tests ---
    [Fact]
    public async Task GetTaskByIdAsync_ReturnsTaskDto_WhenTaskExistsAndBelongsToUser()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var taskItem = new TaskItem { Id = taskId, UserId = Guid.Parse(userId), Title = "Test Task" };
        var taskDto = new TaskDto { Id = taskId, Title = "Test Task" };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        _mockMapper.Setup(m => m.Map<TaskDto>(taskItem)).Returns(taskDto);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ThrowsNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem)null);
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
    }

    [Fact]
    public async Task GetTaskByIdAsync_ThrowsNotFoundException_WhenTaskDoesNotBelongToUser()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var differentUserId = Guid.NewGuid().ToString();

        var taskItem = new TaskItem { Id = taskId, UserId = Guid.Parse(differentUserId), Title = "Test Task" };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
    }

    // --- GetAllTasksAsync tests ---
    [Fact]
    public async Task GetAllTasksAsync_ReturnsPaginatedResult_WhenTasksExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var taskItems = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), UserId = Guid.Parse(userId), Title = "Task 1" },
            new TaskItem { Id = Guid.NewGuid(), UserId = Guid.Parse(userId), Title = "Task 2" }
        };
        var totalCount = taskItems.Count;


        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetAllAsync(paginationParams, userId))
            .ReturnsAsync((taskItems, totalCount));
        _mockMapper.Setup(m => m.Map<IEnumerable<TaskDto>>(taskItems))
            .Returns(taskItems.Select(t => new TaskDto { Id = t.Id, Title = t.Title }));

        // Act
        var result = await _taskService.GetAllTasksAsync(paginationParams);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(paginationParams.PageSize, result.PageSize);
        Assert.Equal(paginationParams.PageNumber, result.CurrentPage);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsEmptyPaginatedResult_WhenTasksDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var taskItems = new List<TaskItem>();
        var totalCount = 0;

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetAllAsync(paginationParams, userId))
           .ReturnsAsync((taskItems, totalCount));

        // Act
        var result = await _taskService.GetAllTasksAsync(paginationParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // --- AddTaskAsync tests ---

    [Fact]
    public async Task AddTaskAsync_WithValidInput_ReturnsTaskDto()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var item = new CreateTaskDto()
        {
            Title = "Title 1",
            Description = "Description 1"
        };
        var taskItem = new TaskItem()
        {
            CreatedAt = DateTime.UtcNow,
            Title = "Title 1",
            Description = "Description 1",
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(p => p.AddAsync(taskItem)).ReturnsAsync(taskItem);
        _mockMapper.Setup(m => m.Map<TaskItem>(item)).Returns(taskItem);
        // Act
        var result = await _taskService.AddTaskAsync(item);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(item.Title, result.Title);
    }

    // --- UpdateTaskAsync tests ---

    // --- DeleteTaskAsync tests ---

    // --- CompleteTaskAsync tests ----
}

