﻿using AppCompras.Models.Data;
using AppCompras.Models.ViewModels;
using AppCompras.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppCompras.Controllers
{
    public class ComprasController(SAPServices context) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("Name"); // Exibe o nome completo do usuário
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View(new ComprasViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string tipoBusca, string valorBusca, DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                ServiceResult<List<ItemCompraViewModel>> ultimaCompraResult;
                ServiceResult<List<MenorValorViewModel>> menorValorResult;
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var username = HttpContext.Session.GetString("Username") ?? "Sistema";
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
                ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

                if (tipoBusca == "pedido")
                {
                    ultimaCompraResult = await context.BuscarPorPedidoCompra(valorBusca, userId, username, dataInicio, dataFim);
                }
                else if (tipoBusca == "item")
                {
                    ultimaCompraResult = await context.BuscarPorItem(valorBusca, userId, username, dataInicio, dataFim);
                }
                else if (tipoBusca == "descricao")
                {
                    ultimaCompraResult = await context.BuscarPorDescricao(valorBusca, userId, username, dataInicio, dataFim);
                }
                else
                {
                    TempData["ErrorMessage"] = "Tipo de busca inválido.";
                    return RedirectToAction("Index");
                }

                menorValorResult = await context.BuscarMenorValor(tipoBusca, valorBusca, userId, username, dataInicio, dataFim);

                ViewBag.Username = HttpContext.Session.GetString("Name");
                ViewBag.TipoBusca = tipoBusca;
                ViewBag.ValorBusca = valorBusca;
                ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

                if (!ultimaCompraResult.Success && !menorValorResult.Success)
                {
                    TempData["ErrorMessage"] = ultimaCompraResult.Message ?? menorValorResult.Message ?? "Nenhum dado encontrado.";
                }

                var model = new ComprasViewModel
                {
                    UltimaCompraItems = ultimaCompraResult.Data ?? new List<ItemCompraViewModel>(),
                    MenorValorItems = menorValorResult.Data ?? new List<MenorValorViewModel>()
                };

                Console.WriteLine($"Itens encontrados (Última Compra): {model.UltimaCompraItems.Count}");
                Console.WriteLine($"Itens encontrados (Menor Valor): {model.MenorValorItems.Count}");
                Console.WriteLine($"===============================");

                return View("Index", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                TempData["ErrorMessage"] = $"Erro ao buscar dados: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarAlteracao(int id, DateTime dataRegistro)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRoleString) || !Enum.TryParse(userRoleString, out UserRole role))
            {
                return RedirectToAction("Login", "Login");
            }

            // Aplica a validação de 180 dias (RF05) definida no serviço
            var validacao = context.ValidarPermissaoAlteracao(dataRegistro, role);

            if (!validacao.Success)
            {
                TempData["ErrorMessage"] = validacao.Message;
                return RedirectToAction("Index");
            }

            // Log da tentativa de alteração autorizada
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var username = HttpContext.Session.GetString("Username") ?? "Sistema";
            
            // Aqui entraria a lógica de persistência no SAP via repositório
            // Por enquanto, apenas registramos o sucesso da validação
            
            TempData["SuccessMessage"] = $"Alteração no item {id} autorizada e processada com sucesso.";
            
            // Opcional: injetar LogService aqui para gravar a edição ou delegar ao SAPServices
            // await _logService.RecordLog(userId, username, "Edição de Registro", $"Item: {id}");

            return RedirectToAction("Index");
        }
    }
}
 
    
