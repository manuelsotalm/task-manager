# AGENTS.md

## Agentic Lab

This repository will be used to practice agentic AI software development process.

## Technology

- .NET 10
- ASP .NET Core Web API
- SQL Server
- Entity Framework
- xUnit

## Architecture

Use the following layers:
- API
- Application
- Domain
- Infrastructure
- Tests

Business rules must not be implemented in controllers.

## Testing

All new business behavior/functionalities must have automated tests.

Before completing a task run:

dotnet build
dotnet test

## Git

Do not commit directly to main or master.

Each feature must use its own branch.

Consider add files to untrack on according to standards to .gitignore. If some file needs to validate with me ask me about that.

## Agent behavior

Before implementing a significant change:

1. inspect the repostory
2. explain the proposed approach
3. identify affected files
4. implement
5. run validation
6. summarize the result