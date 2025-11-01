namespace GameStore.Application.Services
{
  public interface IEncriptService
  {
    string EncodeMaskedCode(string email);
    (string Email, string Expiration)? DecodeMaskedCode(string code);
  }
}
