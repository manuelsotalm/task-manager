namespace TaskManagement.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidStatusTransitionException : DomainException
{
    public TaskManagement.Domain.Enums.TaskStatus FromStatus { get; }
    public TaskManagement.Domain.Enums.TaskStatus ToStatus { get; }

    public InvalidStatusTransitionException(
        TaskManagement.Domain.Enums.TaskStatus fromStatus,
        TaskManagement.Domain.Enums.TaskStatus toStatus)
        : base($"Invalid status transition from '{fromStatus}' to '{toStatus}'.")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }
}

public class TaskNotFoundException : DomainException
{
    public int TaskId { get; }

    public TaskNotFoundException(int taskId)
        : base($"Task with ID {taskId} was not found.")
    {
        TaskId = taskId;
    }
}