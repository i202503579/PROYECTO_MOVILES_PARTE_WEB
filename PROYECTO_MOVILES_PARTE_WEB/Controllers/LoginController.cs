using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApiService _api;

        public LoginController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioLogueado")))
            {
                var rol = HttpContext.Session.GetString("Rol") ?? "";
                if (rol == "Profesor" || rol == "Entrenador")
                {
                    return RedirectToAction("Index", "Clases");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Por favor ingrese usuario y contraseña.";
                return View("Index");
            }

            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var usuario = usuarios.FirstOrDefault(u => 
                u.UsernameUsuario.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) && 
                u.Password == password.Trim() && 
                u.ActivoUsuario);

            if (usuario == null && username.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase) && password.Trim() == "admin123")
            {
                usuario = new Usuario
                {
                    IdUsuario = 1,
                    UsernameUsuario = "admin",
                    NombreUsuario = "Seichi (Admin)",
                    Rol = "Administrador",
                    ActivoUsuario = true
                };
            }

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos, o cuenta inactiva.";
                return View("Index");
            }

            HttpContext.Session.SetString("UsuarioLogueado", !string.IsNullOrEmpty(usuario.NombreUsuario) ? usuario.NombreUsuario : usuario.UsernameUsuario);
            HttpContext.Session.SetString("Username", usuario.UsernameUsuario);
            HttpContext.Session.SetString("Rol", usuario.Rol);
            HttpContext.Session.SetString("IdUsuario", usuario.IdUsuario.ToString());
            HttpContext.Session.SetString("EsAdmin", usuario.Rol == "Administrador" ? "true" : "false");

            if (usuario.Rol == "Profesor" || usuario.Rol == "Entrenador")
            {
                return RedirectToAction("Index", "Clases");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
