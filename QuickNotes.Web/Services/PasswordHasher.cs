using BCrypt.Net;

namespace QuickNotes.Web.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string passwordHash, string inputPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, passwordHash);
        }
    }
}
