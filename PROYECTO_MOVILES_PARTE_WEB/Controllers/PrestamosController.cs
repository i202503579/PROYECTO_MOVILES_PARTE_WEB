using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class PrestamosController : Controller
    {
        private readonly ApiService _api;

        public PrestamosController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var prestamos = await _api.GetListAsync<PrestamoBalon>("PrestamosBalonsAPI/GetPrestamos");
            return View(prestamos.OrderByDescending(p => p.FechaEntrega).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();
            return View(new PrestamoBalon { FechaEntrega = DateTime.Now, EstadoPrestamo = "Pendiente" });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(PrestamoBalon model)
        {
            if (string.IsNullOrWhiteSpace(model.EstadoPrestamo))
            {
                model.EstadoPrestamo = "Pendiente";
            }

            var (ok, mensaje) = await _api.PostAsync("PrestamosBalonsAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Préstamo de balón registrado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo registrar préstamo: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var prestamo = await _api.GetByIdAsync<PrestamoBalon>($"PrestamosBalonsAPI/GetPrestamo/{id}");
            if (prestamo == null) return NotFound();

            await CargarCombos();
            return View(prestamo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(PrestamoBalon model)
        {
            if (model.FechaDevolucion.HasValue && model.FechaDevolucion.Value < model.FechaEntrega)
            {
                ViewBag.Error = "La fecha de devolución no puede ser anterior a la fecha de entrega.";
                await CargarCombos();
                return View(model);
            }

            if (model.EstadoPrestamo != "Pendiente" && !model.FechaDevolucion.HasValue)
            {
                var ahora = DateTime.Now;
                model.FechaDevolucion = ahora < model.FechaEntrega ? model.FechaEntrega : ahora;
            }

            var (ok, mensaje) = await _api.PutAsync("PrestamosBalonsAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Préstamo de balón actualizado correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo actualizar el préstamo: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var prestamo = await _api.GetByIdAsync<PrestamoBalon>($"PrestamosBalonsAPI/GetPrestamo/{id}");
            if (prestamo != null)
            {
                var ahora = DateTime.Now;
                prestamo.FechaDevolucion = ahora < prestamo.FechaEntrega ? prestamo.FechaEntrega : ahora;
                prestamo.EstadoPrestamo = nuevoEstado;

                var (ok, error) = await _api.PutAsync("PrestamosBalonsAPI", prestamo);
                if (ok)
                {
                    TempData["Exito"] = $"Balón registrado como '{nuevoEstado}' correctamente.";
                }
                else
                {
                    TempData["Error"] = $"No se pudo actualizar el estado: {error}";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Devolver(int id)
        {
            return await CambiarEstado(id, "Devuelto");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (ok, error) = await _api.DeleteAsync($"PrestamosBalonsAPI/DeletePrestamo/{id}");
            if (ok)
            {
                TempData["Exito"] = "Préstamo eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = $"No se pudo eliminar el préstamo: {error}";
            }

            return RedirectToAction("Index");
        }

        private async Task CargarCombos()
        {
            var reservas = await _api.GetListAsync<Reserva>("ReservasAPI/GetReservas");
            var clases = await _api.GetListAsync<Clase>("ClasesAPI/GetClases");
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");

            var clientesInstitucionales = clientes.Where(c => c.TipoCliente == "Institucional").Select(c => c.IdCliente).ToHashSet();

            var comboReservas = reservas
                .Where(r => r.EstadoReserva != "Cancelada" && clientesInstitucionales.Contains(r.IdCliente))
                .OrderByDescending(r => r.HoraInicio)
                .Take(25)
                .Select(r => new {
                    Id = r.IdReserva,
                    Texto = $"Alquiler #{r.IdReserva} - {clientes.FirstOrDefault(c => c.IdCliente == r.IdCliente)?.NombreCliente ?? "Institución"} [Institucional] ({r.HoraInicio:dd/MM HH:mm})"
                }).ToList();

            var comboClases = clases
                .Where(c => c.EstadoClase != "Cancelada")
                .OrderByDescending(c => c.FechaClase)
                .Take(25)
                .Select(c => new {
                    Id = c.IdClase,
                    Texto = $"Clase #{c.IdClase} - {canchas.FirstOrDefault(cn => cn.IdCancha == c.IdCancha)?.NombreCancha ?? "Cancha"} ({c.FechaClase:dd/MM} {c.HoraInicio:hh\\:mm})"
                }).ToList();

            ViewBag.Reservas = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(comboReservas, "Id", "Texto");
            ViewBag.Clases = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(comboClases, "Id", "Texto");
        }
    }
}
