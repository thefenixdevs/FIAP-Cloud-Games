using GameStore.Application.Services;
using System.Text;
using System.Web;

namespace GameStore.Infrastructure.Services.Encryption
{
  public class EncriptService : IEncriptService
  {
    public string EncodeMaskedCode(string email)
    {
      if (string.IsNullOrEmpty(email))
        throw new ArgumentNullException(nameof(email), "O email não pode ser nulo ou vazio.");

      var expiration = DateTime.Now.AddMinutes(15);
      var payload = $"{email}|{expiration:o}";

      var bytes = Encoding.UTF8.GetBytes(payload);
      var base64 = Convert.ToBase64String(bytes);
      var masked = HttpUtility.UrlEncode(base64);

      return masked;
    }

    public (string Email, string Expiration)? DecodeMaskedCode(string code)
    {
      try
      {
        var base64 = HttpUtility.UrlDecode(code);
        var bytes = Convert.FromBase64String(base64);
        var payload = Encoding.UTF8.GetString(bytes);
        var parts = payload.Split('|');
        if (parts.Length != 2) return null;

        return (parts[0], parts[1]);
      }
      catch
      {
        return null;
      }
    }
  }
}
