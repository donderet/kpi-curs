using Xunit;
using QuickNotes.Web.Services;
using QuickNotes.Web.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QuickNotes.Tests
{
    public class JwtTokenGeneratorTests
    {
        [Fact]
        public void GenerateToken_ShouldReturnValidJwtString()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "2b7e151628aed2a6abf7158809cf4f3c"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var generator = new JwtTokenGenerator(configuration);
            var user = new User { Id = 1, Username = "admin", PasswordHash = "hash" };

            string token = generator.GenerateToken(user);

            Assert.False(string.IsNullOrEmpty(token));
            
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));
            
            var jwtToken = handler.ReadJwtToken(token);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "admin");
        }
    }
}
