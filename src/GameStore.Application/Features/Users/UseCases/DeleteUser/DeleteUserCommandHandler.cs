using GameStore.Application.Common.Results;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Users.UseCases.DeleteUser;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);

            var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                return ApplicationResult.Failure("UserNotFound");
            }

            await _unitOfWork.Users.DeleteAsync(user.Id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully", request.Id);
            return ApplicationResult.Success("UserDeletedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", request.Id);
            return ApplicationResult.Failure("UserService.DeleteUserAsync.AnErrorOccurredWhileDeletingTheUser");
        }
    }
}

