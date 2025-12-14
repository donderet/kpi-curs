using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickNotes.Web.Models;
using QuickNotes.Web.Repositories;
using System.Security.Claims;

namespace QuickNotes.Web.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly INoteRepository _noteRepository;

        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public IActionResult Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdStr, out int userId))
            {

                return Unauthorized();
            }

            var notes = _noteRepository.GetByUserId(userId);

            return View(notes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string content)
        {

            
            if (string.IsNullOrWhiteSpace(content))
            {

                ModelState.AddModelError("Content", "Content cannot be empty");
                return View();
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdStr, out int userId))
            {

                return Unauthorized();
            }

            var note = new Note
            {
                UserId = userId,
                Content = content
            };

            _noteRepository.Add(note);


            return RedirectToAction("Index");
        }
    }
}
