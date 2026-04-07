using AppCompras.Data;
using AppCompras.Models;
using AppCompras.Models.Data;
using AppCompras.Models.ViewModels;
using AppCompras.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AppCompras.Controllers
{
    public class AdminController(LogService logService, AppDbContext context) : Controller
    {
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == UserRole.Administrador.ToString();
        }

        public async Task<IActionResult> AuditLogs()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem ver os logs.";
                return RedirectToAction("Index", "Compras");
            }

            var logs = await logService.GetLogs();
            ViewBag.Username = HttpContext.Session.GetString("Name");
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem gerenciar usuários.";
                return RedirectToAction("Index", "Compras");
            }

            var users = await context.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync();
            ViewBag.Username = HttpContext.Session.GetString("Name");
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem gerenciar usuários.";
                return RedirectToAction("Index", "Compras");
            }

            ViewBag.Username = HttpContext.Session.GetString("Name");
            return View(new AdminCreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem gerenciar usuários.";
                return RedirectToAction("Index", "Compras");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Username = HttpContext.Session.GetString("Name");
                return View(model);
            }

            var exists = await context.Users.AsNoTracking()
                .AnyAsync(u => u.Username == model.Username);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Username), "Nome de usuário já existe.");
                ViewBag.Username = HttpContext.Session.GetString("Name");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                Status = model.Status
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Usuário criado com sucesso.";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem gerenciar usuários.";
                return RedirectToAction("Index", "Compras");
            }

            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return RedirectToAction("Users");
            }

            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Role = user.Role,
                Status = user.Status
            };

            ViewBag.Username = HttpContext.Session.GetString("Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(AdminEditUserViewModel model)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem gerenciar usuários.";
                return RedirectToAction("Index", "Compras");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Username = HttpContext.Session.GetString("Name");
                return View(model);
            }

            var user = await context.Users.FindAsync(model.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return RedirectToAction("Users");
            }

            var usernameExists = await context.Users.AsNoTracking()
                .AnyAsync(u => u.Username == model.Username && u.Id != model.Id);
            if (usernameExists)
            {
                ModelState.AddModelError(nameof(model.Username), "Nome de usuário já existe.");
                ViewBag.Username = HttpContext.Session.GetString("Name");
                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.Username = model.Username;
            user.Role = model.Role;
            user.Status = model.Status;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Usuário atualizado com sucesso.";
            return RedirectToAction("Users");
        }

    }
}
