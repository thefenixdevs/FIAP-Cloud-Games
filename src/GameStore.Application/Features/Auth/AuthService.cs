using FluentValidation;
using GameStore.Application.Common.Options;
using GameStore.Application.Features.Auth.DTOs;
using GameStore.Application.Features.Auth.Interfaces;
using GameStore.Domain.Aggregates.UserAggregate;
using GameStore.Domain.Aggregates.UserAggregate.Enums;
using GameStore.Domain.Exceptions;
using GameStore.Domain.SeedWork.Behavior;
using GameStore.Domain.Services.EmailService;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameStore.Application.Features.Auth;

public class AuthService(
  IUnitOfWork unitOfWork,
  IJwtService jwtService,
  ILogger<AuthService> logger,
  TypeAdapterConfig mapperConfig,
  IValidator<LoginRequest> loginValidator,
  IEmailService emailService,
  IOptions<BaseUrlOptions> baseUrlOptions) : IAuthService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly TypeAdapterConfig _mapperConfig = mapperConfig;
    private readonly IValidator<LoginRequest> _loginValidator = loginValidator;
    private readonly IEmailService _emailService = emailService;
    private readonly IOptions<BaseUrlOptions> _baseUrlOptions = baseUrlOptions;


  public async Task<LoginResponse> LoginAsync(LoginRequest request)
  {
        await _loginValidator.ValidateAndThrowAsync(request);

        var identifier = request.Identifier.Trim().ToLower();
        User? user = null;
        var lookedUpBy = "identifier";

  
        user = await _unitOfWork.Users.GetByEmailAsync(identifier);
        lookedUpBy = "email";
    
        if(user is null)
        {
            user = await _unitOfWork.Users.GetByUsernameAsync(identifier);
            lookedUpBy = "username";
        }

        if (user == null)
        {
          _logger.LogWarning("Login falhou: usuário com identificador {Identifier} não encontrado", request.Identifier);
          throw new BusinessRulesException(["identifier", "password"], "Credenciais inválidas");
        }

        if (!user.VerifyPassword(request.Password))
        {
          _logger.LogWarning("Login falhou: senha inválida para identificador {Identifier}", request.Identifier);
          throw new BusinessRulesException(["identifier","password"] ,"Credenciais inválidas");
        }

        switch (user.AccountStatus)
        {
          case AccountStatus.Pending:
            _logger.LogWarning("Login falhou: conta pendente de confirmação para {Identifier}", request.Identifier);
            throw new BusinessRulesException("Conta pendente de confirmação de email");
          case AccountStatus.Blocked:
            _logger.LogWarning("Login falhou: conta bloqueada para {Identifier}", request.Identifier);
            throw new BusinessRulesException("Conta bloqueada");
          case AccountStatus.Banned:
            _logger.LogWarning("Login falhou: conta banida para {Identifier}", request.Identifier);
            throw new BusinessRulesException("Conta banida");
        }

        if (user.IsTemporaryPassword)
        {
          _logger.LogWarning("Login falhou: usuário com senha temporária deve alterá-la primeiro {Identifier}", request.Identifier);
          throw new BusinessRulesException("Você deve alterar sua senha temporária antes de fazer login");
        }

        var token = _jwtService.GenerateToken(user);

        var response = user.Adapt<LoginResponse>(_mapperConfig);
        response = response with { Token = token };

        _logger.LogInformation("Usuário {Username} autenticado com sucesso via {Lookup}", user.Username, lookedUpBy);
        return response;
  }

  public async Task ConfirmEmailAsync(Guid userId, string token)
  {
    var user = await _unitOfWork.Users.GetByIdAsync(userId);

    if (user == null)
    {
      _logger.LogWarning("Confirmação de email falhou: usuário não encontrado {UserId}", userId);
      throw new DomainRuleException( "Usuário não encontrado");
    }

    user.ConfirmEmail(token);
    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    _logger.LogInformation("Email confirmado com sucesso para o usuário {UserId}", user.Id);
    return;
  }

  public async Task RequestPasswordResetAsync(RequestPasswordResetRequest request)
  {
    _logger.LogInformation("Solicitação de reset de senha para e-mail: {Email}", request.Email);

    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    if (user == null)
    {
      _logger.LogWarning("Reset de senha falhou: usuário não encontrado para e-mail {Email}", request.Email);
      // Por segurança, não revelamos se o e-mail existe ou não
      return;
    }

    user.GeneratePasswordResetToken();
    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    try
    {
      var baseUrl = _baseUrlOptions.Value.Url;
      var resetLink = $"{baseUrl}/api/auth/confirm-password-reset?userId={user.Id}&token={user.PasswordResetToken}";
      await _emailService.SendPasswordResetAsync(user.Email.Value, user.Name, resetLink);
      _logger.LogInformation("Email de reset de senha enviado para {Email}", user.Email.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de reset de senha para {Email}", user.Email.Value);
      throw;
    }
  }

  public async Task ConfirmPasswordResetAsync(Guid userId, ConfirmPasswordResetRequest request)
  {
    _logger.LogInformation("Confirmação de reset de senha para usuário {UserId}", userId);

    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null)
    {
      _logger.LogWarning("Confirmação de reset de senha falhou: usuário não encontrado {UserId}", userId);
      throw new DomainRuleException("Usuário não encontrado");
    }

    if (request.NewPassword != request.ConfirmPassword)
    {
      _logger.LogWarning("Confirmação de reset de senha falhou: senhas não coincidem para usuário {UserId}", userId);
      throw new DomainRuleException("As senhas não coincidem");
    }

    user.ResetPassword(request.Token, request.NewPassword);
    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    _logger.LogInformation("Senha redefinida com sucesso para o usuário {UserId}", user.Id);
  }

  public async Task RequestEmailChangeAsync(Guid userId, RequestEmailChangeRequest request)
  {
    _logger.LogInformation("Solicitação de troca de e-mail para usuário {UserId}", userId);

    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null)
    {
      _logger.LogWarning("Troca de e-mail falhou: usuário não encontrado {UserId}", userId);
      throw new DomainRuleException("Usuário não encontrado");
    }

    if (await _unitOfWork.Users.ExistsByEmailAsync(request.NewEmail))
    {
      _logger.LogWarning("Troca de e-mail falhou: novo e-mail {NewEmail} já cadastrado", request.NewEmail);
      throw new DomainRuleException("Este e-mail já está cadastrado");
    }

    user.InitiateEmailChange(request.NewEmail);
    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    try
    {
      var baseUrl = _baseUrlOptions.Value.Url;
      var confirmationLink = $"{baseUrl}/api/auth/confirm-email-change?userId={user.Id}&token={user.PendingEmailToken}";
      await _emailService.SendEmailChangeConfirmationAsync(user.Email.Value, user.Name, request.NewEmail, confirmationLink);
      _logger.LogInformation("Email de confirmação de troca enviado para {Email}", user.Email.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de confirmação de troca para {Email}", user.Email.Value);
      throw;
    }
  }

  public async Task ConfirmEmailChangeAsync(Guid userId, string token)
  {
    _logger.LogInformation("Confirmação de troca de e-mail para usuário {UserId}", userId);

    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null)
    {
      _logger.LogWarning("Confirmação de troca de e-mail falhou: usuário não encontrado {UserId}", userId);
      throw new DomainRuleException("Usuário não encontrado");
    }

    user.ConfirmEmailChange(token);
    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    _logger.LogInformation("E-mail alterado com sucesso para o usuário {UserId}", user.Id);
  }

}
