using Microsoft.AspNetCore.Mvc;
using MyWeb.Models;
using MyWeb.Datas;
using Microsoft.AspNetCore.Identity;

namespace MyWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "존재하지 않는 사용자입니다.");
                return View(model);
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "비밀번호가 틀렸습니다.");
                return View(model);
            }

            // 로그인 성공 → 세션 예시
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Index");

            ViewBag.Username = username;
            return View();
        }
    }
}
