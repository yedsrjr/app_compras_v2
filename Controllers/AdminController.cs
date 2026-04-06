using AppCompras.Models.Data;
using AppCompras.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppCompras.Controllers
{
    public class AdminController(LogService logService) : Controller
    {
        public async Task<IActionResult> AuditLogs()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Segurança: Apenas administradores acessam os logs
            if (userRole != UserRole.Administrador.ToString())
            {
                TempData["ErrorMessage"] = "Acesso negado. Apenas administradores podem ver os logs.";
                return RedirectToAction("Index", "Compras");
            }

            var logs = await logService.GetLogs();
            ViewBag.Username = HttpContext.Session.GetString("Name");
            return View(logs);
        }
    }
}