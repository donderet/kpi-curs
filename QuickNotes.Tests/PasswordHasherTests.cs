using Xunit;
using QuickNotes.Web.Services;

namespace QuickNotes.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_ShouldNotReturnPlainPassword()
        {
            var hasher = new PasswordHasher();
            string password = "SecretPassword123!";
            
            string hash = hasher.Hash(password);
            
            Assert.NotEqual(password, hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Verify_ShouldReturnTrue_ForCorrectPassword()
        {
            var hasher = new PasswordHasher();
            string password = "SecretPassword123!";
            string hash = hasher.Hash(password);

            bool result = hasher.Verify(hash, password);

            Assert.True(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForWrongPassword()
        {
            var hasher = new PasswordHasher();
            string password = "SecretPassword123!";
            string hash = hasher.Hash(password);

            bool result = hasher.Verify(hash, "WrongPassword");

            Assert.False(result);
        }
    }
}
