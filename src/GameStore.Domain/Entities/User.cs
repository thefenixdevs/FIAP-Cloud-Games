using GameStore.Domain.Enums;
using GameStore.Domain.Security;
using GameStore.Domain.ValueObjects;

namespace GameStore.Domain.Entities;

public class User : BaseEntity
{
  public string Name { get; private set; } = string.Empty;
  public Email Email { get; private set; } = default;
  public Password Password { get; private set; } = default;
  public string Username { get; private set; } = string.Empty;
  public ProfileType ProfileType { get; private set; }
  public AccountStatus AccountStatus { get; private set; }

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

    public static User Register(string name, string email, string username, string password, IPasswordHasher passwordHasher, ProfileType profileType = ProfileType.CommonUser)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

    var user = new User(Email.Create(email), username, name, profileType);
    user.SetPassword(password, passwordHasher);
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

  public void SetPassword(string password, IPasswordHasher passwordHasher)
  {
    ArgumentNullException.ThrowIfNull(passwordHasher);

    Password = Password.CreateNew(password, passwordHasher);
    UpdatedAt = DateTime.UtcNow;
  }

  public bool VerifyPassword(string password, IPasswordHasher passwordHasher)
  {
    ArgumentNullException.ThrowIfNull(passwordHasher);

    return Password.Verify(password, passwordHasher);
  }

  public void ConfirmAccount()
  {
    if (AccountStatus == AccountStatus.Banned)
    {
      throw new InvalidOperationException("Cannot confirm a banned account.");
    }

    if (AccountStatus == AccountStatus.Active)
    {
      return;
    }

    if (AccountStatus != AccountStatus.Pending)
    {
      throw new InvalidOperationException("Only pending accounts can be confirmed.");
    }

    AccountStatus = AccountStatus.Active;
    UpdatedAt = DateTime.UtcNow;
  }

  public void BlockAccount()
  {
    if (AccountStatus == AccountStatus.Banned)
    {
      throw new InvalidOperationException("Cannot block a banned account.");
    }

    if (AccountStatus == AccountStatus.Blocked)
    {
      return;
    }

    if (AccountStatus != AccountStatus.Active)
    {
      throw new InvalidOperationException("Only active accounts can be blocked.");
    }

    AccountStatus = AccountStatus.Blocked;
    UpdatedAt = DateTime.UtcNow;
  }

  public void UnblockAccount()
  {
    if (AccountStatus != AccountStatus.Blocked)
    {
      throw new InvalidOperationException("Only blocked accounts can be unblocked.");
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
      throw new ArgumentException("Name must be provided.", nameof(name));
    }

    Name = name.Trim();
  }

  private void SetUsername(string username)
  {
    Username = UsernameNormalizer.Normalize(username);
  }
}
