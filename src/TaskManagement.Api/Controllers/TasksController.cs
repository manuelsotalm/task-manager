namespace TaskManagement.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateTaskCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all tasks, optionally filtered by status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks(
        [FromQuery] TaskStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTasksQuery(status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTask(int id, CancellationToken cancellationToken)
    {
        var query = new GetTaskQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result is null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Updates a task's details.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateTaskCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (TaskNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Changes a task's status.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> ChangeStatus(int id, [FromBody] ChangeTaskStatusDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ChangeTaskStatusCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (TaskNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidStatusTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteTaskCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (TaskNotFoundException)
        {
            return NotFound();
        }
    }
}