namespace TaskManagement.Application.Validators;

using FluentValidation;
using TaskManagement.Application.Commands;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 4).WithMessage("Priority must be between 0 and 4.")
            .When(x => x.Priority.HasValue);
    }
}

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 4).WithMessage("Priority must be between 0 and 4.")
            .When(x => x.Priority.HasValue);
    }
}

public class ChangeTaskStatusDtoValidator : AbstractValidator<ChangeTaskStatusDto>
{
    public ChangeTaskStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.TaskDto).SetValidator(new CreateTaskDtoValidator());
    }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task id must be greater than 0.");
        RuleFor(x => x.TaskDto).SetValidator(new UpdateTaskDtoValidator());
    }
}

public class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task id must be greater than 0.");
        RuleFor(x => x.StatusDto).SetValidator(new ChangeTaskStatusDtoValidator());
    }
}

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task id must be greater than 0.");
    }
}

public class GetTaskQueryValidator : AbstractValidator<GetTaskQuery>
{
    public GetTaskQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task id must be greater than 0.");
    }
}
