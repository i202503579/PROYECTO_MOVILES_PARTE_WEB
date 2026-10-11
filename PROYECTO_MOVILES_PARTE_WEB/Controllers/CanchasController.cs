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

        public async Task<IActionResult> Index()
        {
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            return View(canchas);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Cancha { ActivoCancha = true });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cancha model)
        {
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
            var cancha = await _api.GetByIdAsync<Cancha>($"CanchasAPI/GetCancha/{id}");
            if (cancha == null) return NotFound();
            return View(cancha);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Cancha model)
        {
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
