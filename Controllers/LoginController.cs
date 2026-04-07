using AppCompras.Models;
using AppCompras.Models.ViewModels;
using AppCompras.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppCompras.Controllers
{
    public class LoginController(UserServices user_context) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var result = await user_context.Login(username, password);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View("Index");
            }

            TempData["SuccessMessage"] = result.Message;

            HttpContext.Session.SetInt32("UserId", result.Data!.Id);
            HttpContext.Session.SetString("Name", result.Data.Name);
            HttpContext.Session.SetString("Username", result.Data.Username);
            HttpContext.Session.SetString("UserRole", result.Data.Role.ToString());

            return RedirectToAction("Index", "Compras");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Faça login para alterar sua senha.";
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("Name");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Faça login para alterar sua senha.";
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Username = HttpContext.Session.GetString("Name");
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
                return View(model);
            }

            var result = await user_context.ChangePassword(
                userId.Value,
                model.CurrentPassword,
                model.NewPassword,
                model.ConfirmPassword);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.Username = HttpContext.Session.GetString("Name");
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "Compras");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await user_context.ResetPasswordByLogin(
                model.Username,
                model.Email,
                model.NewPassword,
                model.ConfirmPassword);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);
            
            var result = await user_context.Register(model);

            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? "", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "Login");

        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Sessão encerrada com sucesso.";
            return RedirectToAction("Index", "Login");
        }
    }
}
