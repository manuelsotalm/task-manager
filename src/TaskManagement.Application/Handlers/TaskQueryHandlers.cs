namespace TaskManagement.Application.Handlers;

using MediatR;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries;
using TaskDomain = TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskDto?>
{
    private readonly ITaskRepository _repository;

    public GetTaskQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskDto?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return task is null ? null : MapToDto(task);
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

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public GetTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TaskDomain.Task> tasks;

        if (request.Status.HasValue)
            tasks = await _repository.GetByStatusAsync(request.Status.Value, cancellationToken);
        else
            tasks = await _repository.GetAllAsync(cancellationToken);

        return tasks.Select(MapToDto).ToList();
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