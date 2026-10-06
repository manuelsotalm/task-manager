namespace TaskManagement.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Tests.Fixtures;
using Xunit;

public class TasksControllerTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TasksControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Clear database before each test
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up after each test
        await InitializeAsync();
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated_WithValidData()
    {
        // Arrange
        var createDto = new CreateTaskDto("Test Task", "Test Description", 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Title.Should().Be("Test Task");
        task.Description.Should().Be("Test Description");
        task.Priority.Should().Be(1);
        task.Status.Should().Be(TaskStatus.Pending);
        task.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WithEmptyTitle()
    {
        // Arrange
        var createDto = new CreateTaskDto("", "Description", 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WithTitleTooLong()
    {
        // Arrange
        var createDto = new CreateTaskDto(new string('a', 101), "Description", 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WithInvalidPriority()
    {
        // Arrange
        var createDto = new CreateTaskDto("Valid Title", "Description", 5);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTasks_ReturnsOk_WithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasks_ReturnsOk_WithTasks()
    {
        // Arrange
        await CreateTaskAsync("Task 1", "Desc 1", 0);
        await CreateTaskAsync("Task 2", "Desc 2", 2);

        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
        tasks!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTasks_ReturnsFilteredByStatus()
    {
        // Arrange
        var task1 = await CreateTaskAsync("Pending Task", "Desc", 0);
        var task2 = await CreateTaskAsync("InProgress Task", "Desc", 1);
        await ChangeStatusAsync(task2.Id, TaskStatus.InProgress);

        // Act
        var response = await _client.GetAsync("/api/tasks?status=Pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
        tasks!.Should().HaveCount(1);
        tasks[0].Title.Should().Be("Pending Task");
    }

    [Fact]
    public async Task GetTask_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Id.Should().Be(created.Id);
        task.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_ReturnsOk_WithValidData()
    {
        // Arrange
        var created = await CreateTaskAsync("Original Title", "Original Desc", 3);

        // Act
        var updateDto = new UpdateTaskDto("Updated Title", "Updated Desc", 1);
        var response = await _client.PutAsJsonAsync($"/api/tasks/{created.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Title.Should().Be("Updated Title");
        task.Description.Should().Be("Updated Desc");
        task.Priority.Should().Be(1);
        task.UpdatedAt.Should().BeAfter(created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateTaskDto("Updated Title", "Updated Desc", 1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeStatus_ReturnsOk_WithValidTransition()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);

        // Act: Pending -> InProgress
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task ChangeStatus_ReturnsBadRequest_WithInvalidTransition()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);
        // First move to InProgress
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));
        // Then move to Completed
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Completed));

        // Act: Try to move from Completed -> InProgress (invalid)
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_AllowsPendingToCancelled()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);

        // Act: Pending -> Cancelled
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Cancelled));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task!.Status.Should().Be(TaskStatus.Cancelled);
    }

    [Fact]
    public async Task ChangeStatus_AllowsInProgressToCancelled()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Act: InProgress -> Cancelled
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Cancelled));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task!.Status.Should().Be(TaskStatus.Cancelled);
    }

    [Fact]
    public async Task ChangeStatus_AllowsInProgressToCompleted()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Act: InProgress -> Completed
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Completed));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task!.Status.Should().Be(TaskStatus.Completed);
    }

    [Fact]
    public async Task ChangeStatus_DoesNotAllowCompletedToInProgress()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Completed));

        // Act: Completed -> InProgress (invalid)
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_DoesNotAllowCancelledToInProgress()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.Cancelled));

        // Act: Cancelled -> InProgress (invalid)
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.PatchAsJsonAsync("/api/tasks/999/status", new ChangeTaskStatusDto(TaskStatus.InProgress));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        var created = await CreateTaskAsync("Test Task", "Description", 2);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Helper methods
    private async Task<TaskDto> CreateTaskAsync(string title, string? description = null, int? priority = null)
    {
        var createDto = new CreateTaskDto(title, description, priority);
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>())!;
    }

    private async Task<TaskDto> ChangeStatusAsync(int id, TaskStatus status)
    {
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{id}/status", new ChangeTaskStatusDto(status));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>())!;
    }
}