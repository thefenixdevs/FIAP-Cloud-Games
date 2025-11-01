using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Auth.UseCases.SendAccountConfirmation;

public sealed record SendAccountConfirmationCommand(
    string Email) : IRequest<ApplicationResult>;

