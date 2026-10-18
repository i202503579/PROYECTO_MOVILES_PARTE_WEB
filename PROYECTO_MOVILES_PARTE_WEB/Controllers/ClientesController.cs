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

        public async Task<IActionResult> Index(string? buscar, int pagina = 1)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = clientes.Where(c => 
                    c.NombreCliente.Contains(buscar, StringComparison.OrdinalIgnoreCase) || 
                    c.NumeroDocumento.Contains(buscar)).ToList();
            }

            var clientesOrdenados = clientes.OrderBy(c => c.IdCliente).ToList();

            const int tamanoPagina = 10;
            var totalRegistros = clientesOrdenados.Count;
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var clientesPaginados = clientesOrdenados.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();

            ViewBag.Buscar = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TamanoPagina = tamanoPagina;

            return View(clientesPaginados);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

            return View(new Cliente { ActivoCliente = true, TipoCliente = "Particular" });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cliente model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

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
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

            var cliente = await _api.GetByIdAsync<Cliente>($"ClientesAPI/GetCliente/{id}");
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Cliente model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

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
