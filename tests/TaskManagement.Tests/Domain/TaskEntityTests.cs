namespace TaskManagement.Tests.Domain;

using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.ValueObjects;
using Xunit;

public class TaskEntityTests
{
    [Fact]
    public void Constructor_CreatesTaskWithDefaults()
    {
        // Act
        var task = new Task("Test Task");

        // Assert
        task.Title.Should().Be("Test Task");
        task.Description.Should().BeNull();
        task.Status.Should().Be(TaskStatus.Pending);
        task.Priority.Value.Should().Be(2); // Medium
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_CreatesTaskWithCustomValues()
    {
        // Act
        var task = new Task("Test Task", "Description", Priority.High);

        // Assert
        task.Title.Should().Be("Test Task");
        task.Description.Should().Be("Description");
        task.Priority.Value.Should().Be(1); // High
    }

    [Fact]
    public void Constructor_Throws_WhenTitleIsEmpty()
    {
        // Act & Assert
        var action = () => new Task("");
        action.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty.*");
    }

    [Fact]
    public void Constructor_Throws_WhenTitleIsWhitespace()
    {
        // Act & Assert
        var action = () => new Task("   ");
        action.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty.*");
    }

    [Fact]
    public void Constructor_Throws_WhenTitleExceeds100Chars()
    {
        // Act & Assert
        var action = () => new Task(new string('a', 101));
        action.Should().Throw<ArgumentException>().WithMessage("Title cannot exceed 100 characters.*");
    }

    [Fact]
    public void UpdateDetails_UpdatesFields()
    {
        // Arrange
        var task = new Task("Original", "Original Desc", Priority.Low);

        // Act
        task.UpdateDetails("Updated", "Updated Desc", Priority.High);

        // Assert
        task.Title.Should().Be("Updated");
        task.Description.Should().Be("Updated Desc");
        task.Priority.Value.Should().Be(1); // High
        task.UpdatedAt.Should().BeAfter(task.CreatedAt);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenTitleIsEmpty()
    {
        // Arrange
        var task = new Task("Original");

        // Act & Assert
        var action = () => task.UpdateDetails("", "Desc");
        action.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty.*");
    }

    [Theory]
    [InlineData(TaskStatus.Pending, TaskStatus.InProgress, true)]
    [InlineData(TaskStatus.Pending, TaskStatus.Cancelled, true)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Completed, true)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Cancelled, true)]
    [InlineData(TaskStatus.Completed, TaskStatus.InProgress, false)]
    [InlineData(TaskStatus.Completed, TaskStatus.Cancelled, false)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.InProgress, false)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.Completed, false)]
    [InlineData(TaskStatus.Pending, TaskStatus.Completed, false)]
    public void CanTransitionTo_ReturnsExpectedResult(TaskStatus from, TaskStatus to, bool expected)
    {
        // Arrange
        var task = new Task("Test");
        // Use reflection to set status for testing
        var statusField = typeof(Task).GetField("<Status>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField!.SetValue(task, from);

        // Act
        var result = task.CanTransitionTo(to);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(TaskStatus.Pending, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Pending, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Completed)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Cancelled)]
    public void ChangeStatus_Succeeds_ForValidTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = new Task("Test");
        var statusField = typeof(Task).GetField("<Status>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField!.SetValue(task, from);

        // Act
        task.ChangeStatus(to);

        // Assert
        task.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(TaskStatus.Completed, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Completed, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.Completed)]
    [InlineData(TaskStatus.Pending, TaskStatus.Completed)]
    public void ChangeStatus_Throws_ForInvalidTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = new Task("Test");
        var statusField = typeof(Task).GetField("<Status>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField!.SetValue(task, from);

        // Act & Assert
        var action = () => task.ChangeStatus(to);
        action.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage($"Invalid status transition from '{from}' to '{to}'.");
    }

    [Fact]
    public void ChangeStatus_DoesNothing_WhenStatusIsSame()
    {
        // Arrange
        var task = new Task("Test");
        var originalUpdatedAt = task.UpdatedAt;

        // Act
        task.ChangeStatus(TaskStatus.Pending);

        // Assert
        task.Status.Should().Be(TaskStatus.Pending);
        task.UpdatedAt.Should().Be(originalUpdatedAt);
    }
}