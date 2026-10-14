using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class UsuariosController : Controller
    {
        private readonly ApiService _api;

        public UsuariosController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            return View(usuarios);
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

            return View(new Usuario { ActivoUsuario = true, Rol = "Operador" });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuario model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var (ok, mensaje) = await _api.PostAsync("UsuariosAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Usuario creado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo crear usuario: {mensaje}";
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

            var usuario = await _api.GetByIdAsync<Usuario>($"UsuariosAPI/GetUsuario/{id}");
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Usuario model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol != "Administrador")
            {
                TempData["Error"] = "Acceso exclusivo para administradores.";
                return RedirectToAction("Index", rol == "Profesor" || rol == "Entrenador" ? "Clases" : "Home");
            }

            var (ok, mensaje) = await _api.PutAsync("UsuariosAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Usuario modificado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al modificar: {mensaje}";
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

            var usuario = await _api.GetByIdAsync<Usuario>($"UsuariosAPI/GetUsuario/{id}");
            if (usuario != null)
            {
                usuario.ActivoUsuario = !usuario.ActivoUsuario;
                await _api.PutAsync("UsuariosAPI", usuario);
                TempData["Exito"] = "Estado del usuario actualizado.";
            }
            return RedirectToAction("Index");
        }
    }
}
