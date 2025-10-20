using FluentValidation;
using GameStore.Application.Common.Options;
using GameStore.Application.Features.Users.DTOs;
using GameStore.Application.Features.Users.Interfaces;
using GameStore.Domain.Aggregates.UserAggregate;
using GameStore.Domain.Exceptions;
using GameStore.Domain.SeedWork.Behavior;
using GameStore.Domain.Services.EmailService;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameStore.Application.Features.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOptions<BaseUrlOptions> _baseUrlOptions;
    private readonly TypeAdapterConfig _mapperConfig;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserValidator;

    public UserService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UserService> logger,
        IConfiguration configuration,
        IOptions<BaseUrlOptions> baseUrlOptions,
        TypeAdapterConfig mapperConfig,
        IValidator<RegisterRequest> registerValidator,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserRequest> updateUserValidator)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
        _baseUrlOptions = baseUrlOptions;
        _mapperConfig = mapperConfig;
        _registerValidator = registerValidator;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Validando Formulário de registro");
        await _registerValidator.ValidateAndThrowAsync(request);

        await _unitOfWork.BeginTransactionAsync();
        _logger.LogInformation("Formulário de registro validado com sucesso");
        var user = request.Adapt<User>(_mapperConfig);
        user.GenerateEmailConfirmationToken();

        await _unitOfWork.Users.AddAsync(user);

        try
        {
            var baseUrl = _baseUrlOptions.Value.Url;
            var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";
            await _emailService.SendEmailConfirmationAsync(user.Email.Value, user.Name, confirmationLink);
            _logger.LogInformation("Email de confirmação enviado para {Email}", user.Email.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de confirmação para {Email}", user.Email.Value);
            // Continua o registro mesmo se o email falhar - o usuário pode solicitar reenvio depois
        }

        await _unitOfWork.CommitTransactionAsync();
        _logger.LogInformation("Usuário {Username} registrado com sucesso com ID {UserId}", user.Username, user.Id);
       
    }

  public async Task<IEnumerable<UserManagementResponse>> GetAllUsersAsync()
  {
    _logger.LogInformation("Fetching all users");

    var users = await _unitOfWork.Users.GetAllAsync();
    return users.Select(user => user.Adapt<UserManagementResponse>(_mapperConfig));
  }

  public async Task<UserManagementResponse?> GetUserByIdAsync(Guid id)
  {
    _logger.LogInformation("Fetching user with ID: {UserId}", id);

    var user = await _unitOfWork.Users.GetByIdAsync(id);
    if (user is null)
    {
      _logger.LogWarning("User with ID {UserId} not found", id);
      return null;
    }

    return user.Adapt<UserManagementResponse>(_mapperConfig);
  }

  public async Task<UserManagementResponse> CreateUserAsync(CreateUserRequest request)
  {
    await ValidateRequestAsync(_createUserValidator, request);

    _logger.LogInformation("Criando novo usuário: {Username}", request.Username);

    if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
    {
      _logger.LogWarning("Criação de usuário falhou: email {Email} já cadastrado", request.Email);
      throw new DomainRuleException("Email já cadastrado");
    }

    if (await _unitOfWork.Users.ExistsByUsernameAsync(request.Username))
    {
      _logger.LogWarning("Criação de usuário falhou: username {Username} já cadastrado", request.Username);
      throw new DomainRuleException("Username já cadastrado");
    }

    var user = request.Adapt<User>(_mapperConfig);
    user.GenerateEmailConfirmationToken();
    user.GeneratePasswordResetToken();
    user.SetTemporaryPassword(request.Password);

    await _unitOfWork.Users.AddAsync(user);
    await _unitOfWork.CommitAsync();
  
    try
    {
      var baseUrl = _baseUrlOptions.Value.Url;
      var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";
      var resetLink = $"{baseUrl}/api/auth/confirm-password-reset?userId={user.Id}&token={user.PasswordResetToken}";
      await _emailService.SendTemporaryPasswordAsync(user.Email.Value, user.Name, user.Email.Value, request.Password, resetLink);
      _logger.LogInformation("Email de senha temporária enviado para {Email}", user.Email.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Falha ao enviar email de senha temporária para {Email}", user.Email.Value);
    }

    _logger.LogInformation("Usuário criado com sucesso com ID: {UserId}", user.Id);
    return user.Adapt<UserManagementResponse>(_mapperConfig);
  }

  public async Task UpdateUserAsync(Guid id, UpdateUserRequest request)
  {
    await ValidateRequestAsync(_updateUserValidator, request);

    _logger.LogInformation("Atualizando usuário com ID: {UserId}", id);

    var user = await _unitOfWork.Users.GetByIdAsync(id);
    if (user is null)
    {
      _logger.LogWarning("Usuário com ID {UserId} não encontrado", id);
      throw new DomainRuleException("Usuário não encontrado");
    }

    if(user.Email.Value != request.Email && await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
    {
      _logger.LogWarning("Atualização de usuário falhou: email {Email} já cadastrado", request.Email);
      throw new DomainRuleException("Email já cadastrado");
    }

    if (user.Username != request.Username && await _unitOfWork.Users.ExistsByUsernameAsync(request.Username))
    {
      _logger.LogWarning("Atualização de usuário falhou: username {Username} já cadastrado", request.Username);
      throw new DomainRuleException("Username já cadastrado");
    }

    user = User.Update(user, request.Name, request.Email, request.Username, request.AccountStatus, request.ProfileType);

    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.CommitAsync();

    _logger.LogInformation("Usuário com ID {UserId} atualizado com sucesso", id);
    return;
  }

  public async Task DeleteUserAsync(Guid id)
  {
    _logger.LogInformation("Excluindo usuário com ID: {UserId}", id);

    var user = await _unitOfWork.Users.GetByIdAsync(id);
    if (user is null)
    {
      _logger.LogWarning("Usuário com ID {UserId} não encontrado", id);
      throw new DomainRuleException("Usuário não encontrado");
    }

    await _unitOfWork.Users.DeleteAsync(user.Id);
    await _unitOfWork.CommitAsync();

    _logger.LogInformation("Usuário com ID {UserId} excluído com sucesso", id);
    return;
  }

  private async Task ValidateRequestAsync<T>(IValidator<T> validator, T request)
  {
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }
  }
}