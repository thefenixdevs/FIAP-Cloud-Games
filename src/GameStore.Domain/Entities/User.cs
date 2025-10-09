using GameStore.Domain.Enums;

namespace GameStore.Domain.Entities;

public class User : BaseEntity
{
  public string Name { get; set; }
  public string Email { get; private set; }
  public string PasswordHash { get; private set; }
  public string Username { get; private set; }
  public ProfileType ProfileType { get; private set; }
  public AccountStatus AccountStatus { get; private set; }


  private User()
  {
    Name = string.Empty;
    Email = string.Empty;
    PasswordHash = string.Empty;
    Username = string.Empty;
  }

  public User(string email, string username, string passwordHash, ProfileType profileType = ProfileType.CommonUser)
  {
    Id = Guid.NewGuid();
    Name = username;
    Email = email;
    Username = username;
    PasswordHash = passwordHash;
    ProfileType = profileType;
    AccountStatus = AccountStatus.Pending;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public void UpdateProfile(string username)
  {
    Username = username;
    UpdatedAt = DateTime.UtcNow;
  }

  public void ConfirmAccount()
  {
    AccountStatus = AccountStatus.Confirmed;
    UpdatedAt = DateTime.UtcNow;
  }

  public void BanAccount()
  {
    AccountStatus = AccountStatus.Banned;
    UpdatedAt = DateTime.UtcNow;
  }

  public void SetProfileType(ProfileType profileType)
  {
    ProfileType = profileType;
    UpdatedAt = DateTime.UtcNow;
  }
}
