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
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);
        _mockMapper.Setup(m => m.Map<TaskDto>(taskItem)).Returns(taskDto);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, CancellationToken.None);

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
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem)null);
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.GetTaskByIdAsync(taskId, CancellationToken.None));
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
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.GetTaskByIdAsync(taskId, CancellationToken.None));
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
        _mockTaskRepository.Setup(r => r.GetAllAsync(paginationParams, userId, CancellationToken.None))
            .ReturnsAsync((taskItems, totalCount));
        _mockMapper.Setup(m => m.Map<IEnumerable<TaskDto>>(taskItems))
            .Returns(taskItems.Select(t => new TaskDto { Id = t.Id, Title = t.Title }));

        // Act
        var result = await _taskService.GetAllTasksAsync(paginationParams, CancellationToken.None);
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
        _mockTaskRepository.Setup(r => r.GetAllAsync(paginationParams, userId, It.IsAny<CancellationToken>()))
           .ReturnsAsync((taskItems, totalCount));

        // Act
        var result = await _taskService.GetAllTasksAsync(paginationParams, CancellationToken.None);

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
        TaskItem? capturedTaskItem = null;

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);

        _mockTaskRepository.Setup(p => p.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
             .Callback<TaskItem, CancellationToken>((ti, ct) => capturedTaskItem = ti)
             .ReturnsAsync((TaskItem ti, CancellationToken ct) => ti);

        _mockMapper.Setup(m => m.Map<TaskItem>(item))
            .Returns((CreateTaskDto src) => new TaskItem { Title = src.Title, Description = src.Description });
        _mockMapper.Setup(m => m.Map<TaskDto>(It.IsAny<TaskItem>()))
                   .Returns((TaskItem ti) => new TaskDto { Id = ti.Id, Title = ti.Title, Description = ti.Description, IsCompleted = ti.IsCompleted });
        // Act
        var result = await _taskService.AddTaskAsync(item, CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(item.Title, result.Title);
        Assert.Equal(item.Description, result.Description);
        Assert.NotNull(result.Id);
        Assert.False(result.IsCompleted);
        Assert.NotNull(capturedTaskItem);
        Assert.Equal(item.Title, capturedTaskItem.Title);
        Assert.Equal(item.Description, capturedTaskItem.Description);
        Assert.Equal(Guid.Parse(userId), capturedTaskItem.UserId);
        Assert.NotEqual(Guid.Empty, capturedTaskItem.Id);
    }

    [Fact]
    public async Task AddTaskAsync_WhenRepositoryFails_ThrowsException()
    {
        // Arrange
        var userIdString = Guid.NewGuid().ToString();
        var createTaskDto = new CreateTaskDto { Title = "Task to fail" };
        var expectedException = new InvalidOperationException("Simulated repository failure");

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userIdString);
        _mockMapper.Setup(m => m.Map<TaskItem>(createTaskDto)).Returns(new TaskItem());
        _mockTaskRepository.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);

        // Act & Assert
        var caughtException = await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.AddTaskAsync(createTaskDto, CancellationToken.None));
        Assert.Equal(expectedException, caughtException);
    }

    // --- UpdateTaskAsync tests ---

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTask_WhenTaskExistsAndBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem()
        {
            UserId = userId,
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description"
        };

        var updateTask = new UpdateTaskDto()
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);
        _mockTaskRepository.Setup(r => r.UpdateAsync(taskItem, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map(updateTask, taskItem));

        // Act
        await _taskService.UpdateTaskAsync(taskId, updateTask, CancellationToken.None);

        // Assert
        _mockTaskRepository.Verify(r => r.UpdateAsync(taskItem, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowNotFoundException_WhenTaskDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem()
        {
            UserId = otherUserId,
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description"
        };

        var updateTask = new UpdateTaskDto()
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);
        _mockTaskRepository.Setup(r => r.UpdateAsync(taskItem, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map(updateTask, taskItem));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.UpdateTaskAsync(taskId, updateTask, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var updateTask = new UpdateTaskDto()
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.UpdateTaskAsync(taskId, updateTask, CancellationToken.None));
    }

    // --- DeleteTaskAsync tests ---

    [Fact]
    public async Task DeleteTaskAsync_DeletesTask_WhenTaskExistAndBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem()
        {
            Id = taskId,
            UserId = userId,
            Title = "Title",
            Description = "Description"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);
        _mockTaskRepository.Setup(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        // Act
        await _taskService.DeleteTaskAsync(taskId, CancellationToken.None);

        // Assert
        _mockTaskRepository.Verify(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ThrowsNotFoundExceptiob_WhenTaskDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var taskItem = new TaskItem()
        {
            Id = taskId,
            UserId = anotherUserId,
            Title = "Title",
            Description = "Description"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.DeleteTaskAsync(taskId, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteTaskAsync_ThrowsNotFoundExceptiob_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.DeleteTaskAsync(taskId, CancellationToken.None));
        _mockTaskRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- CompleteTaskAsync tests ----

    [Fact]
    public async Task CompleteTaskAsync_UpdatesIsCompleted_WhenTaskExistsAndBelongsToUserAndIsNotAlreadyCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var taskItem = new TaskItem
        {
            Id = taskId,
            UserId = Guid.Parse(userId),
            Title = "Incomplete Task",
            IsCompleted = false,
            CompletedAt = null
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);
        _mockTaskRepository.Setup(r => r.UpdateAsync(taskItem, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _taskService.CompleteTaskAsync(taskId, CancellationToken.None);

        // Assert
        Assert.True(taskItem.IsCompleted);
        Assert.NotNull(taskItem.CompletedAt);
    }

    [Fact]
    public async Task CompleteTaskAsync_DoesNothing_WhenTaskIsAlreadyCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var completedAt = DateTime.UtcNow.AddDays(-1);
        var taskItem = new TaskItem { Id = taskId, UserId = Guid.Parse(userId), Title = "Completed Task", IsCompleted = true, CompletedAt = completedAt };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);

        // Act
        await _taskService.CompleteTaskAsync(taskId, CancellationToken.None);

        // Assert
        _mockTaskRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.True(taskItem.IsCompleted);
        Assert.Equal(completedAt, taskItem.CompletedAt);
    }

    [Fact]
    public async Task CompleteTaskAsync_ThrowsNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.CompleteTaskAsync(taskId, CancellationToken.None));
    }

    [Fact]
    public async Task CompleteTaskAsync_ThrowsNotFoundException_WhenTaskDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var taskItem = new TaskItem()
        {
            UserId = anotherUserId,
            Id = taskId,
            Title = "My Title"
        };

        _mockCurrentUserProvider.Setup(p => p.GetCurrentUserId()).Returns(userId.ToString());
        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(taskItem);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _taskService.CompleteTaskAsync(taskId, CancellationToken.None));
    }

}

