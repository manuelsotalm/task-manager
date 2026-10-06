namespace TaskManagement.Domain.Repositories;

using TaskEntity = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using System.Threading.Tasks;

public interface ITaskRepository
{
    Task<TaskEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskEntity>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default);
    Task<TaskEntity> AddAsync(TaskEntity task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}