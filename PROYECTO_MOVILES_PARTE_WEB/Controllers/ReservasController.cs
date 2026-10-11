using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class ReservasController : Controller
    {
        private readonly ApiService _api;

        public ReservasController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? estado, int? idCancha)
        {
            var reservas = await _api.GetListAsync<Reserva>("ReservasAPI/GetReservas");
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");

            foreach (var r in reservas)
            {
                r.NombreCliente = clientes.FirstOrDefault(c => c.IdCliente == r.IdCliente)?.NombreCliente ?? $"Cliente #{r.IdCliente}";
                r.NombreCancha = canchas.FirstOrDefault(c => c.IdCancha == r.IdCancha)?.NombreCancha ?? $"Cancha #{r.IdCancha}";
                r.NombreUsuario = usuarios.FirstOrDefault(u => u.IdUsuario == r.IdUsuario)?.NombreUsuario ?? $"Usuario #{r.IdUsuario}";
            }

            if (!string.IsNullOrEmpty(estado))
            {
                reservas = reservas.Where(r => r.EstadoReserva.Equals(estado, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (idCancha.HasValue && idCancha.Value > 0)
            {
                reservas = reservas.Where(r => r.IdCancha == idCancha.Value).ToList();
            }

            ViewBag.EstadoSel = estado;
            ViewBag.IdCanchaSel = idCancha;
            ViewBag.Canchas = canchas;
            ViewBag.TotalRecaudado = reservas.Where(r => r.EstadoReserva != "Cancelada").Sum(r => r.MontoRecibido);
            ViewBag.TotalPorCobrar = reservas.Where(r => r.EstadoReserva != "Cancelada").Sum(r => Math.Max(0, r.PrecioReserva - r.MontoRecibido));

            return View(reservas.OrderByDescending(r => r.HoraInicio).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();
            var reserva = new Reserva
            {
                HoraInicio = DateTime.Now.Date.AddHours(DateTime.Now.Hour + 1),
                HoraFin = DateTime.Now.Date.AddHours(DateTime.Now.Hour + 2),
                EstadoReserva = "Pendiente"
            };
            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Reserva model)
        {
            var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");
            if (model.IdUsuario == 0 && int.TryParse(idUsuarioSesion, out int idU))
            {
                model.IdUsuario = idU;
            }

            var (ok, mensaje) = await _api.PostAsync("ReservasAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Reserva registrada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"No se pudo crear la reserva: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var reserva = await _api.GetByIdAsync<Reserva>($"ReservasAPI/GetReserva/{id}");
            if (reserva == null)
            {
                return NotFound();
            }

            await CargarCombos();
            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Reserva model)
        {
            var (ok, mensaje) = await _api.PutAsync("ReservasAPI", model);
            if (ok)
            {
                TempData["Exito"] = "Reserva modificada correctamente.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = $"Error al modificar: {mensaje}";
            await CargarCombos();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var reserva = await _api.GetByIdAsync<Reserva>($"ReservasAPI/GetReserva/{id}");
            if (reserva != null)
            {
                reserva.EstadoReserva = "Cancelada";
                await _api.PutAsync("ReservasAPI", reserva);
                TempData["Exito"] = "Reserva cancelada correctamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ReporteImprimible()
        {
            var reservas = await _api.GetListAsync<Reserva>("ReservasAPI/GetReservas");
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");

            foreach (var r in reservas)
            {
                r.NombreCliente = clientes.FirstOrDefault(c => c.IdCliente == r.IdCliente)?.NombreCliente ?? $"Cliente #{r.IdCliente}";
                r.NombreCancha = canchas.FirstOrDefault(c => c.IdCancha == r.IdCancha)?.NombreCancha ?? $"Cancha #{r.IdCancha}";
            }

            return View(reservas.Where(r => r.EstadoReserva != "Cancelada").OrderByDescending(r => r.HoraInicio).ToList());
        }

        private async Task CargarCombos()
        {
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");

            ViewBag.Clientes = new SelectList(clientes.Where(c => c.ActivoCliente), "IdCliente", "NombreCliente");
            ViewBag.Canchas = new SelectList(canchas.Where(c => c.ActivoCancha), "IdCancha", "NombreCancha");
            ViewBag.ListaCanchas = canchas.Where(c => c.ActivoCancha).ToList();
            ViewBag.Usuarios = new SelectList(usuarios.Where(u => u.ActivoUsuario && (u.Rol == "Administrador" || u.Rol == "Operador")), "IdUsuario", "NombreUsuario");
        }
    }
}
