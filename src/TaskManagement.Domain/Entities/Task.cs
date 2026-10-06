namespace TaskManagement.Domain.Entities;

using System.Diagnostics.CodeAnalysis;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.ValueObjects;
using TaskManagement.Domain.Exceptions;

public class Task
{
    private static readonly Dictionary<TaskStatus, HashSet<TaskStatus>> ValidTransitions = new()
    {
        [TaskStatus.Pending] = new() { TaskStatus.InProgress, TaskStatus.Cancelled },
        [TaskStatus.InProgress] = new() { TaskStatus.Completed, TaskStatus.Cancelled },
        [TaskStatus.Completed] = new(),
        [TaskStatus.Cancelled] = new()
    };

    public int Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public byte[]? RowVersion { get; private set; }

    private Task() { } // EF Core

    [SetsRequiredMembers]
    public Task(string title, string? description = null, Priority? priority = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters.", nameof(title));

        Title = title;
        Description = description;
        Status = TaskStatus.Pending;
        Priority = priority ?? Priority.Medium;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, Priority? priority = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters.", nameof(title));

        Title = title;
        Description = description;
        if (priority.HasValue)
            Priority = priority.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        if (Status == newStatus)
            return;

        if (!ValidTransitions[Status].Contains(newStatus))
            throw new InvalidStatusTransitionException(Status, newStatus);

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanTransitionTo(TaskStatus newStatus) => ValidTransitions[Status].Contains(newStatus);

    public static IReadOnlyDictionary<TaskStatus, HashSet<TaskStatus>> GetValidTransitions() => ValidTransitions;
}