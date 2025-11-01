using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using GameStore.Domain.Repositories.Abstractions;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Users.UseCases.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApplicationResult<IEnumerable<UserManagementResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult<IEnumerable<UserManagementResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all users");

            var users = await _unitOfWork.Users.GetAllAsync();
            var response = users.Adapt<IEnumerable<UserManagementResponse>>();

            return ApplicationResult<IEnumerable<UserManagementResponse>>.Success(response, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return ApplicationResult<IEnumerable<UserManagementResponse>>.Failure("An error occurred while fetching users");
        }
    }
}

