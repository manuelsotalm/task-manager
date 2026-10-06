namespace TaskManagement.Application.Commands;

using MediatR;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;

public record CreateTaskCommand(CreateTaskDto TaskDto) : IRequest<TaskDto>;

public record UpdateTaskCommand(int Id, UpdateTaskDto TaskDto) : IRequest<TaskDto>;

public record ChangeTaskStatusCommand(int Id, ChangeTaskStatusDto StatusDto) : IRequest<TaskDto>;

public record DeleteTaskCommand(int Id) : IRequest<Unit>;