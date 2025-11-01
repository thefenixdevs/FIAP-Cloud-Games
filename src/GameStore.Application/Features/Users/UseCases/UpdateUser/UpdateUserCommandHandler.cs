using GameStore.Application.Common.Results;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Users.UseCases.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", request.Id);

            var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                return ApplicationResult.Failure("UserNotFound");
            }

            // Validações de unicidade são feitas no validator via FluentValidation
            user = User.Update(user, request.Name, request.Email, request.Username, request.AccountStatus, request.ProfileType);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User with ID {UserId} updated successfully", request.Id);
            return ApplicationResult.Success("UserUpdatedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", request.Id);
            return ApplicationResult.Failure("UserService.UpdateUserAsync.AnErrorOccurredWhileUpdatingTheUser");
        }
    }
}

