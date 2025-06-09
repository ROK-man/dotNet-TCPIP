using Microsoft.AspNetCore.Mvc;
using MyWeb.Models;
using MyWeb.Datas;
using Microsoft.AspNetCore.Identity;

namespace MyWeb.Controllers
{
    public class RegisterController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public RegisterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "이미 존재하는 사용자 이름입니다.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = _hasher.HashPassword(null, model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
