using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class AlumnosController : Controller
    {
        private readonly ApiService _api;

        public AlumnosController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            int.TryParse(HttpContext.Session.GetString("IdUsuario"), out int idUsuarioLogueado);

            var alumnos = await _api.GetListAsync<Alumno>("AlumnosAPI/GetAlumnos");
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");

            if (rol == "Profesor" || rol == "Entrenador")
            {
                alumnos = alumnos.Where(a => a.IdUsuario == idUsuarioLogueado).ToList();
            }

            foreach (var a in alumnos)
            {
                a.NombreProfesor = usuarios.FirstOrDefault(u => u.IdUsuario == a.IdUsuario)?.NombreUsuario ?? $"Profesor #{a.IdUsuario}";
            }

            return View(alumnos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                TempData["Error"] = "No tiene permisos para matricular alumnos.";
                return RedirectToAction("Index");
            }

            await CargarProfesores();
            return View(new Alumno { ActivoAlumno = true, FechaIngreso = DateOnly.FromDateTime(DateTime.Now) });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Alumno model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                TempData["Error"] = "No tiene permisos para matricular alumnos.";
                return RedirectToAction("Index");
            }

            var (ok, mensaje) = await _api.PostAsync("AlumnosAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Alumno registrado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo registrar alumno: {mensaje}";
            await CargarProfesores();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                TempData["Error"] = "No tiene permisos para modificar datos de alumnos.";
                return RedirectToAction("Index");
            }

            var alumno = await _api.GetByIdAsync<Alumno>($"AlumnosAPI/GetAlumno/{id}");
            if (alumno == null) return NotFound();

            await CargarProfesores();
            return View(alumno);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Alumno model)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            if (rol == "Profesor" || rol == "Entrenador")
            {
                TempData["Error"] = "No tiene permisos para modificar datos de alumnos.";
                return RedirectToAction("Index");
            }

            var (ok, mensaje) = await _api.PutAsync("AlumnosAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Alumno modificado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al modificar: {mensaje}";
            await CargarProfesores();
            return View(model);
        }

        private async Task CargarProfesores()
        {
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var profesores = usuarios.Where(u => u.ActivoUsuario && u.Rol == "Profesor").ToList();
            ViewBag.Profesores = new SelectList(profesores, "IdUsuario", "NombreUsuario");
        }
    }
}
