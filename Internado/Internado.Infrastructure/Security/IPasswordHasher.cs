namespace Internado.Infrastructure.Security
{
    public interface IPasswordHasher
    {
        string HashToBase64(string plainPassword, int workFactor = 12);
        bool VerifyFromBase64(string plainPassword, string hashBase64);
    }
}
