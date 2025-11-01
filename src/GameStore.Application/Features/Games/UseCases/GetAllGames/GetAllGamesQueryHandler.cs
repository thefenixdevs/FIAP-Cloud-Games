using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using GameStore.Domain.Repositories.Abstractions;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Games.UseCases.GetAllGames;

public sealed class GetAllGamesQueryHandler : IRequestHandler<GetAllGamesQuery, ApplicationResult<IEnumerable<GameResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllGamesQueryHandler> _logger;

    public GetAllGamesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllGamesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult<IEnumerable<GameResponse>>> Handle(GetAllGamesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all games");

            var games = await _unitOfWork.Games.GetAllAsync();
            var response = games.Adapt<IEnumerable<GameResponse>>();

            return ApplicationResult<IEnumerable<GameResponse>>.Success(response, "Games retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all games");
            return ApplicationResult<IEnumerable<GameResponse>>.Failure("An error occurred while fetching games");
        }
    }
}

