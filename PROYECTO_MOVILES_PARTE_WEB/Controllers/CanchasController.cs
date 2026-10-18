using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class CanchasController : Controller
    {
        private readonly ApiService _api;

        public CanchasController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int pagina = 1)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            var canchasOrdenadas = canchas.OrderBy(c => c.IdCancha).ToList();

            const int tamanoPagina = 10;
            var totalRegistros = canchasOrdenadas.Count;
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var canchasPaginadas = canchasOrdenadas.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TamanoPagina = tamanoPagina;

            return View(canchasPaginadas);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            return View(new Cancha { ActivoCancha = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cancha model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var (ok, mensaje) = await _api.PostAsync("CanchasAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Cancha registrada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al registrar cancha: {mensaje}";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var cancha = await _api.GetByIdAsync<Cancha>($"CanchasAPI/GetCancha/{id}");
            if (cancha == null) return NotFound();
            return View(cancha);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Cancha model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var (ok, mensaje) = await _api.PutAsync("CanchasAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Cancha actualizada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al actualizar: {mensaje}";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var cancha = await _api.GetByIdAsync<Cancha>($"CanchasAPI/GetCancha/{id}");
            if (cancha != null)
            {
                cancha.ActivoCancha = !cancha.ActivoCancha;
                await _api.PutAsync("CanchasAPI", cancha);
                TempData["Exito"] = $"Estado de la cancha cambiado a {(cancha.ActivoCancha ? "Activa" : "Inactiva")}.";
            }
            return RedirectToAction("Index");
        }
    }
}
