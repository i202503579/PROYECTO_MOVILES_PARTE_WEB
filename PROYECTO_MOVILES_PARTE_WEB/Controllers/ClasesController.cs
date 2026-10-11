using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class ClasesController : Controller
    {
        private readonly ApiService _api;

        public ClasesController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var clases = await _api.GetListAsync<Clase>("ClasesAPI/GetClases");
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");

            foreach (var c in clases)
            {
                c.NombreProfesor = usuarios.FirstOrDefault(u => u.IdUsuario == c.IdUsuario)?.NombreUsuario ?? $"Profesor #{c.IdUsuario}";
                c.NombreCancha = canchas.FirstOrDefault(ch => ch.IdCancha == c.IdCancha)?.NombreCancha ?? $"Cancha #{c.IdCancha}";
            }

            return View(clases.OrderByDescending(c => c.FechaClase).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();
            var clase = new Clase
            {
                FechaClase = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                HoraInicio = new TimeOnly(15, 0),
                HoraFin = new TimeOnly(17, 0),
                EstadoClase = "Programada",
                ActivoClase = true
            };
            return View(clase);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Clase model)
        {
            var (ok, mensaje) = await _api.PostAsync("ClasesAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Clase programada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo programar la clase: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var clase = await _api.GetByIdAsync<Clase>($"ClasesAPI/GetClase/{id}");
            if (clase == null) return NotFound();

            await CargarCombos();
            return View(clase);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Clase model)
        {
            var (ok, mensaje) = await _api.PutAsync("ClasesAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Clase modificada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al modificar: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Asistencia(int id)
        {
            var clase = await _api.GetByIdAsync<Clase>($"ClasesAPI/GetClase/{id}");
            if (clase == null) return NotFound();

            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            var alumnos = await _api.GetListAsync<Alumno>("AlumnosAPI/GetAlumnos");
            var asistencias = await _api.GetListAsync<Asistencia>("AsistenciasAPI/GetAsistencias");

            clase.NombreProfesor = usuarios.FirstOrDefault(u => u.IdUsuario == clase.IdUsuario)?.NombreUsuario ?? $"Profesor #{clase.IdUsuario}";
            clase.NombreCancha = canchas.FirstOrDefault(ch => ch.IdCancha == clase.IdCancha)?.NombreCancha ?? $"Cancha #{clase.IdCancha}";

            var alumnosDelProfesor = alumnos.Where(a => a.IdUsuario == clase.IdUsuario && a.ActivoAlumno).ToList();
            var asistenciasDeLaClase = asistencias.Where(a => a.IdClase == id).ToList();

            var viewModel = new AsistenciaClaseViewModel
            {
                Clase = clase,
                Alumnos = alumnosDelProfesor.Select(a =>
                {
                    var regAsistencia = asistenciasDeLaClase.FirstOrDefault(ast => ast.IdAlumno == a.IdAlumno);
                    return new AsistenciaItemViewModel
                    {
                        IdAlumno = a.IdAlumno,
                        NombreAlumno = a.NombreAlumno,
                        TelefonoApoderado = a.TelefonoApoderado,
                        IdAsistencia = regAsistencia?.IdAsistencia,
                        EstadoAsistencia = regAsistencia?.EstadoAsistencia ?? "Presente"
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(int idClase, List<AsistenciaItemViewModel> alumnos, bool marcarCompletada = false)
        {
            if (alumnos != null && alumnos.Count > 0)
            {
                foreach (var item in alumnos)
                {
                    var reg = new Asistencia
                    {
                        IdAsistencia = item.IdAsistencia ?? 0,
                        IdClase = idClase,
                        IdAlumno = item.IdAlumno,
                        EstadoAsistencia = string.IsNullOrEmpty(item.EstadoAsistencia) ? "Presente" : item.EstadoAsistencia
                    };

                    if (item.IdAsistencia.HasValue && item.IdAsistencia.Value > 0)
                    {
                        await _api.PutAsync("AsistenciasAPI", reg);
                    }
                    else
                    {
                        await _api.PostAsync("AsistenciasAPI", reg);
                    }
                }
            }

            if (marcarCompletada)
            {
                var clase = await _api.GetByIdAsync<Clase>($"ClasesAPI/GetClase/{idClase}");
                if (clase != null)
                {
                    clase.EstadoClase = "Completada";
                    await _api.PutAsync("ClasesAPI", clase);
                }
            }

            TempData["Exito"] = "Asistencia registrada y guardada exitosamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var clase = await _api.GetByIdAsync<Clase>($"ClasesAPI/GetClase/{id}");
            if (clase != null)
            {
                clase.EstadoClase = "Cancelada";
                await _api.PutAsync("ClasesAPI", clase);
                TempData["Exito"] = "Clase cancelada correctamente.";
            }

            return RedirectToAction("Index");
        }

        private async Task CargarCombos()
        {
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");

            var profesores = usuarios.Where(u => u.ActivoUsuario && u.Rol == "Profesor").ToList();
            ViewBag.Profesores = new SelectList(profesores, "IdUsuario", "NombreUsuario");
            ViewBag.Canchas = new SelectList(canchas.Where(c => c.ActivoCancha), "IdCancha", "NombreCancha");
        }
    }
}
