namespace TaskManagement.Application.DTOs;

using TaskManagement.Domain.Enums;
using TaskManagement.Domain.ValueObjects;

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskStatus Status,
    int Priority,
    string PriorityDisplay,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskDto(
    string Title,
    string? Description,
    int? Priority = null
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    int? Priority = null
);

public record ChangeTaskStatusDto(
    TaskStatus Status
);