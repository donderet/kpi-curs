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

            if (content.Length > 1000)
            {
                ModelState.AddModelError("Content", "Content cannot exceed 1000 characters");
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId) || note.UserId != userId)
            {
                return Unauthorized();
            }

            return View(note);
        }

        [HttpPost]
        public IActionResult Edit(int id, string content)
        {
            var note = _noteRepository.GetById(id);
            if (note == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId) || note.UserId != userId)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("Content", "Content cannot be empty");
                return View(note);
            }

            if (content.Length > 1000)
            {
                ModelState.AddModelError("Content", "Content cannot exceed 1000 characters");
                return View(note);
            }

            note.Content = content;
            _noteRepository.Update(note);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId) || note.UserId != userId)
            {
                return Unauthorized();
            }

            _noteRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
