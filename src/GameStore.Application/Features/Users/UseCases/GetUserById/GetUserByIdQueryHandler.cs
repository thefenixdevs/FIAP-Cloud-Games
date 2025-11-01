using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using GameStore.Domain.Repositories.Abstractions;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Users.UseCases.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApplicationResult<UserManagementResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult<UserManagementResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", request.Id);

            var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                return ApplicationResult<UserManagementResponse>.Failure("UserNotFound");
            }

            var response = user.Adapt<UserManagementResponse>();
            return ApplicationResult<UserManagementResponse>.Success(response, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", request.Id);
            return ApplicationResult<UserManagementResponse>.Failure("An error occurred while fetching the user");
        }
    }
}

