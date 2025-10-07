using GameStore.Domain.Enums;

namespace GameStore.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Username { get; private set; }
    public ProfileType ProfileType { get; private set; }
    public AccountStatus AccountStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Username = string.Empty;
    }

    public User(string email, string username, string passwordHash, ProfileType profileType = ProfileType.CommonUser)
    {
        Id = Guid.NewGuid();
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
