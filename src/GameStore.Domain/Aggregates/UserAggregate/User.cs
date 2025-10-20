using GameStore.Domain.Aggregates.UserAggregate.Enums;
using GameStore.Domain.Aggregates.UserAggregate.ValueObjects;
using GameStore.Domain.Exceptions;
using GameStore.Domain.SeedWork.Entities;

namespace GameStore.Domain.Aggregates.UserAggregate;

public class User : BaseEntity
{
  public string Name { get; private set; } = string.Empty;
  public Email Email { get; private set; } = default;
  public Password Password { get; private set; } = default;
  public string Username { get; private set; } = string.Empty;
  public ProfileType ProfileType { get; private set; }
  public AccountStatus AccountStatus { get; private set; }
  public string? EmailConfirmationToken { get; private set; }
  public DateTime? EmailConfirmationTokenExpiresAt { get; private set; }
  public DateTime? EmailConfirmedAt { get; private set; }
  public string? PasswordResetToken { get; private set; }
  public DateTime? PasswordResetTokenExpiresAt { get; private set; }
  public bool IsTemporaryPassword { get; private set; }
  public string? PendingEmail { get; private set; }
  public string? PendingEmailToken { get; private set; }

  private User()
  {
    AccountStatus = AccountStatus.Pending;
  }

  private User(Email email, string username, string name, ProfileType profileType) : this()
  {
    Id = Guid.NewGuid();
    Email = email;
    SetUsername(username);
    SetName(name);
    ProfileType = profileType;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = CreatedAt;
  }

    private User(string name, Email email, string username, ProfileType profileType) : this()
    {
        SetName(name);
        Email = email;
        Username = username;
        ProfileType = profileType;
        UpdatedAt = DateTime.UtcNow;
    }

    public static User Register(string name, string email, string username, string password, ProfileType profileType = ProfileType.CommonUser)
    {
        var user = new User(Email.Create(email), username, name, profileType);
        user.SetPassword(password);
        return user;
    }

    public static User Update(User user, string name, string email, string username, AccountStatus AccountStatus, ProfileType profileType = ProfileType.CommonUser)
    {
        user.Name = name;
        user.Email = Email.Create(email);
        user.Username = username;
        user.ProfileType = profileType;
        switch (AccountStatus)
        {
            case AccountStatus.Active:
                user.ConfirmAccount();
                break;
            case AccountStatus.Blocked: 
                user.BlockAccount(); 
                break;
            case AccountStatus.Banned:
                user.BanAccount();
                break;
        }
        return user;
    }

  public void SetPassword(string password)
  {
    Password = Password.CreateNew(password);
    UpdatedAt = DateTime.UtcNow;
  }

  public bool VerifyPassword(string password)
  {
    return Password.Verify(password);
  }

  public void GenerateEmailConfirmationToken()
  {
    EmailConfirmationToken = Guid.NewGuid().ToString();
    UpdatedAt = DateTime.UtcNow;
  }

  public void ConfirmEmail(string token)
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      throw new DomainRuleException("token", "Token de confirmação é obrigatório");
    }

    if (EmailConfirmedAt.HasValue)
    {
      throw new DomainRuleException("email", "Email já foi confirmado");
    }

    if (EmailConfirmationToken != token)
    {
      throw new DomainRuleException("token", "Token de confirmação inválido");
    }

    EmailConfirmedAt = DateTime.UtcNow;
    EmailConfirmationToken = null;
    UpdatedAt = DateTime.UtcNow;
    
    // Ativar automaticamente a conta após confirmação do email
    ConfirmAccount();
  }

  public void ConfirmAccount()
  {
    if (AccountStatus == AccountStatus.Banned)
    {
      throw new DomainRuleException("Não é possível confirmar uma conta banida");
    }

    if (AccountStatus == AccountStatus.Active)
    {
      return;
    }

    if (AccountStatus != AccountStatus.Pending)
    {
      throw new DomainRuleException("Apenas contas pendentes podem ser confirmadas");
    }

    if (!EmailConfirmedAt.HasValue)
    {
      throw new DomainRuleException("O email deve ser confirmado antes de ativar a conta");
    }

    AccountStatus = AccountStatus.Active;
    UpdatedAt = DateTime.UtcNow;
  }

  public void BlockAccount()
  {
    if (AccountStatus == AccountStatus.Banned)
    {
      throw new DomainRuleException("Não é possível bloquear uma conta banida");
    }

    if (AccountStatus == AccountStatus.Blocked)
    {
      return;
    }

    if (AccountStatus != AccountStatus.Active)
    {
      throw new DomainRuleException("Apenas contas ativas podem ser bloqueadas");
    }

    AccountStatus = AccountStatus.Blocked;
    UpdatedAt = DateTime.UtcNow;
  }

  public void UnblockAccount()
  {
    if (AccountStatus != AccountStatus.Blocked)
    {
      throw new DomainRuleException("Apenas contas bloqueadas podem ser desbloqueadas");
    }

    AccountStatus = AccountStatus.Active;
    UpdatedAt = DateTime.UtcNow;
  }

  public void BanAccount()
  {
    if (AccountStatus == AccountStatus.Banned)
    {
      return;
    }

    AccountStatus = AccountStatus.Banned;
    UpdatedAt = DateTime.UtcNow;
  }

  public void UpdateProfile(string username, string? name = null)
  {
    SetUsername(username);

    if (!string.IsNullOrWhiteSpace(name))
    {
      SetName(name);
    }

    UpdatedAt = DateTime.UtcNow;
  }

  public void SetProfileType(ProfileType profileType)
  {
    ProfileType = profileType;
    UpdatedAt = DateTime.UtcNow;
  }

  private void SetName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainRuleException("Name", "Nome deve ser fornecido");
    }

    Name = name.Trim();
  }

  private void SetUsername(string username)
  {
    Username = UsernameNormalizer.Normalize(username);
  }

  public void GeneratePasswordResetToken()
  {
    PasswordResetToken = Guid.NewGuid().ToString();
    PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(5);
    UpdatedAt = DateTime.UtcNow;
  }

  public void ResetPassword(string token, string newPassword)
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      throw new DomainRuleException("token", "Token de reset de senha é obrigatório");
    }

    if (PasswordResetToken != token)
    {
      throw new DomainRuleException("token", "Token de reset de senha inválido");
    }

    if (!PasswordResetTokenExpiresAt.HasValue || DateTime.UtcNow > PasswordResetTokenExpiresAt.Value)
    {
      throw new DomainRuleException("token", "Token de reset de senha expirado");
    }

    SetPassword(newPassword);
    PasswordResetToken = null;
    PasswordResetTokenExpiresAt = null;
    IsTemporaryPassword = false;
    UpdatedAt = DateTime.UtcNow;
  }

  public void SetTemporaryPassword(string password)
  {
    SetPassword(password);
    IsTemporaryPassword = true;
    UpdatedAt = DateTime.UtcNow;
  }

  public void InitiateEmailChange(string newEmail)
  {
    if (string.IsNullOrWhiteSpace(newEmail))
    {
      throw new DomainRuleException("email", "Novo e-mail é obrigatório");
    }

    PendingEmail = newEmail;
    PendingEmailToken = Guid.NewGuid().ToString();
    UpdatedAt = DateTime.UtcNow;
  }

  public void ConfirmEmailChange(string token)
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      throw new DomainRuleException("token", "Token de confirmação de e-mail é obrigatório");
    }

    if (string.IsNullOrWhiteSpace(PendingEmail))
    {
      throw new DomainRuleException("email", "Nenhuma troca de e-mail pendente");
    }

    if (PendingEmailToken != token)
    {
      throw new DomainRuleException("token", "Token de confirmação de e-mail inválido");
    }

    Email = Email.Create(PendingEmail);
    PendingEmail = null;
    PendingEmailToken = null;
    UpdatedAt = DateTime.UtcNow;
  }
}
