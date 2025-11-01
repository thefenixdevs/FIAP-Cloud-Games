using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Users.UseCases.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApplicationResult<UserManagementResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateUserCommandHandler> logger,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async ValueTask<ApplicationResult<UserManagementResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Username}", request.Username);

            // Validações de unicidade são feitas no validator via FluentValidation
            var user = User.Register(request.Name, request.Email, request.Username, request.Password, _passwordHasher, request.ProfileType);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

            var response = user.Adapt<UserManagementResponse>();

            return ApplicationResult<UserManagementResponse>.Success(response, "UserRegisteredSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", request.Username);
            return ApplicationResult<UserManagementResponse>.Failure("UserService.CreateUserAsync.AnErrorOccurredWhileCreatingTheUser");
        }
    }
}

