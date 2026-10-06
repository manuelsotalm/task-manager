namespace TaskManagement.Application.Handlers;

using MediatR;
using TaskManagement.Application.Commands;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskDomain = TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using TaskManagement.Domain.ValueObjects;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(ITaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var priority = request.TaskDto.Priority.HasValue
            ? new Priority(request.TaskDto.Priority.Value)
            : Priority.Medium;

        var task = new TaskDomain.Task(
            request.TaskDto.Title,
            request.TaskDto.Description,
            priority
        );

        var created = await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(created);
    }

    private static TaskDto MapToDto(TaskDomain.Task task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority.Value,
        task.Priority.ToDisplayString(),
        task.CreatedAt,
        task.UpdatedAt
    );
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(ITaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new TaskNotFoundException(request.Id);

        var priority = request.TaskDto.Priority.HasValue
            ? new Priority(request.TaskDto.Priority.Value)
            : (Priority?)null;

        task.UpdateDetails(request.TaskDto.Title, request.TaskDto.Description, priority);

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    private static TaskDto MapToDto(TaskDomain.Task task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority.Value,
        task.Priority.ToDisplayString(),
        task.CreatedAt,
        task.UpdatedAt
    );
}

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTaskStatusCommandHandler(ITaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new TaskNotFoundException(request.Id);

        task.ChangeStatus(request.StatusDto.Status);

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    private static TaskDto MapToDto(TaskDomain.Task task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority.Value,
        task.Priority.ToDisplayString(),
        task.CreatedAt,
        task.UpdatedAt
    );
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(ITaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.Id, cancellationToken);
        if (!exists)
            throw new TaskNotFoundException(request.Id);

        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}