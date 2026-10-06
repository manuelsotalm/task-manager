namespace TaskManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskEntity = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using TaskManagement.Domain.Repositories;
using TaskManagement.Infrastructure.Persistence;
using System.Threading.Tasks;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity> AddAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Tasks.AddAsync(task, cancellationToken);
        return entry.Entity;
    }

    public async Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object?[] { id }, cancellationToken);
        if (task != null)
        {
            _context.Tasks.Remove(task);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken);
    }
}