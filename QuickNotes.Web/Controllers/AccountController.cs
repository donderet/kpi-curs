using Microsoft.AspNetCore.Mvc;
using QuickNotes.Web.Models;
using QuickNotes.Web.Repositories;
using QuickNotes.Web.Services;
using QuickNotes.Web.ViewModels;

namespace QuickNotes.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountController(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_userRepository.GetByUsername(model.Username) != null)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = _passwordHasher.Hash(model.Password)
            };
            
            _userRepository.Add(user);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _userRepository.GetByUsername(model.Username);
            if (user == null || !_passwordHasher.Verify(user.PasswordHash, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt");
                return View(model);
            }

            var token = _jwtTokenGenerator.GenerateToken(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }
    }
}
