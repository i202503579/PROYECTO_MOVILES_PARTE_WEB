using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class ClientesController : Controller
    {
        private readonly ApiService _api;

        public ClientesController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? buscar)
        {
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = clientes.Where(c => 
                    c.NombreCliente.Contains(buscar, StringComparison.OrdinalIgnoreCase) || 
                    c.NumeroDocumento.Contains(buscar)).ToList();
            }
            ViewBag.Buscar = buscar;
            return View(clientes);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Cliente { ActivoCliente = true, TipoCliente = "Particular" });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cliente model)
        {
            var (ok, mensaje) = await _api.PostAsync("ClientesAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Cliente registrado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al registrar cliente: {mensaje}";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _api.GetByIdAsync<Cliente>($"ClientesAPI/GetCliente/{id}");
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Cliente model)
        {
            var (ok, mensaje) = await _api.PutAsync("ClientesAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Cliente modificado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al modificar cliente: {mensaje}";
            return View(model);
        }
    }
}
