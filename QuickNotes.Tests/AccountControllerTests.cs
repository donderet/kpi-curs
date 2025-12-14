using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using QuickNotes.Web.Controllers;
using QuickNotes.Web.Services;
using QuickNotes.Web.Repositories;
using QuickNotes.Web.ViewModels;
using QuickNotes.Web.Models;
using Microsoft.AspNetCore.Http;

namespace QuickNotes.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly Mock<IJwtTokenGenerator> _mockJwt;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockHasher = new Mock<IPasswordHasher>();
            _mockJwt = new Mock<IJwtTokenGenerator>();
            _controller = new AccountController(_mockRepo.Object, _mockHasher.Object, _mockJwt.Object);
            
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void Register_Post_ValidUser_ShouldCreateUserAndRedirect()
        {
            var model = new RegisterViewModel { Username = "newuser", Password = "password" };
            _mockRepo.Setup(r => r.GetByUsername("newuser")).Returns((User?)null);
            _mockHasher.Setup(h => h.Hash("password")).Returns("hashed_password");

            var result = _controller.Register(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            _mockRepo.Verify(r => r.Add(It.Is<User>(u => u.Username == "newuser" && u.PasswordHash == "hashed_password")), Times.Once);
        }

        [Fact]
        public void Login_Post_ValidCredentials_ShouldSetCookieAndRedirect()
        {
            var model = new LoginViewModel { Username = "user", Password = "password" };
            var user = new User { Id = 1, Username = "user", PasswordHash = "hash" };
            
            _mockRepo.Setup(r => r.GetByUsername("user")).Returns(user);
            _mockHasher.Setup(h => h.Verify("hash", "password")).Returns(true);
            _mockJwt.Setup(j => j.GenerateToken(user)).Returns("valid_token");

            var result = _controller.Login(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            

        }

        [Fact]
        public void Login_Post_InvalidCredentials_ShouldReturnViewWithError()
        {
             var model = new LoginViewModel { Username = "user", Password = "wrong" };
            var user = new User { Id = 1, Username = "user", PasswordHash = "hash" };
            
            _mockRepo.Setup(r => r.GetByUsername("user")).Returns(user);
            _mockHasher.Setup(h => h.Verify("hash", "wrong")).Returns(false);

            var result = _controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }
    }
}
