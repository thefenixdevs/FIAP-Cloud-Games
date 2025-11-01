using GameStore.Application.Common.Results;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Domain.ValueObjects;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Auth.UseCases.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, ApplicationResult<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async ValueTask<ApplicationResult<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var identifier = request.Identifier.Trim();
            User? user = null;
            var lookedUpBy = "identifier";

            try
            {
                var email = Email.Create(identifier);
                user = await _unitOfWork.Users.GetByEmailAsync(email.Value);
                lookedUpBy = "email";
            }
            catch (ArgumentException)
            {
                user = await _unitOfWork.Users.GetByUsernameAsync(identifier);
                lookedUpBy = "username";
            }

            if (user == null)
            {
                _logger.LogWarning("Login failed: User with identifier {Identifier} not found", request.Identifier);
                return ApplicationResult<LoginResponse>.ValidationFailure(new[] { "identifier", "password" }, "AuthService.LoginAsync.InvalidCredentials", "AuthService.LoginAsync.InvalidCredentials");
            }

            if (!user.VerifyPassword(request.Password, _passwordHasher))
            {
                _logger.LogWarning("Login failed: Invalid password for identifier {Identifier}", request.Identifier);
                return ApplicationResult<LoginResponse>.ValidationFailure(new[] { "identifier", "password" }, "AuthService.LoginAsync.InvalidCredentials", "AuthService.LoginAsync.InvalidCredentials");
            }

            switch (user.AccountStatus)
            {
                case AccountStatus.Pending:
                    _logger.LogWarning("Login failed: Account pending confirmation for identifier {Identifier}", request.Identifier);
                    return ApplicationResult<LoginResponse>.Failure("AuthService.LoginAsync.AccountPendingEmailConfirmation");
                case AccountStatus.Blocked:
                    _logger.LogWarning("Login failed: Account is blocked for identifier {Identifier}", request.Identifier);
                    return ApplicationResult<LoginResponse>.Failure("AuthService.LoginAsync.AccountIsBlocked");
                case AccountStatus.Banned:
                    _logger.LogWarning("Login failed: Account is banned for identifier {Identifier}", request.Identifier);
                    return ApplicationResult<LoginResponse>.Failure("AuthService.LoginAsync.AccountIsBanned");
            }

            var token = _jwtService.GenerateToken(user);

            // Mapear User para LoginResponse usando Mapster e depois adicionar Token
            var response = user.Adapt<LoginResponse>();
            response = response with { Token = token };

            _logger.LogInformation("User {Username} logged in successfully using {Lookup}", user.Username, lookedUpBy);
            return ApplicationResult<LoginResponse>.Success(response, "AuthService.LoginAsync.LoginSuccessful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for identifier {Identifier}", request.Identifier);
            return ApplicationResult<LoginResponse>.Failure("AuthService.LoginAsync.AnErrorOccurredDuringLogin");
        }
    }
}

