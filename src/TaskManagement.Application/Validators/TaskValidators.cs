namespace TaskManagement.Application.Validators;

using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.ValueObjects;

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