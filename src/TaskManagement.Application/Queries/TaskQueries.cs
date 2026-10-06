namespace TaskManagement.Application.Queries;

using MediatR;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;

public record GetTaskQuery(int Id) : IRequest<TaskDto?>;

public record GetTasksQuery(TaskStatus? Status = null) : IRequest<IReadOnlyList<TaskDto>>;