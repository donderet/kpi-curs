using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using QuickNotes.Web.Controllers;
using QuickNotes.Web.Repositories;
using QuickNotes.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace QuickNotes.Tests
{
    public class NotesControllerTests
    {
        private readonly Mock<INoteRepository> _mockRepo;
        private readonly NotesController _controller;

        public NotesControllerTests()
        {
            _mockRepo = new Mock<INoteRepository>();
            _controller = new NotesController(_mockRepo.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
            }, "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public void Index_ShouldReturnViewWithUserNotes()
        {
            var notes = new List<Note> { new Note { Id = 1, UserId = 1, Content = "Note 1" } };
            _mockRepo.Setup(r => r.GetByUserId(1)).Returns(notes);

            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Note>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_Post_ValidNote_ShouldAddAndRedirect()
        {
            string content = "New Note";

            var result = _controller.Create(content);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockRepo.Verify(r => r.Add(It.Is<Note>(n => n.UserId == 1 && n.Content == content)), Times.Once);
        }
    }
}
