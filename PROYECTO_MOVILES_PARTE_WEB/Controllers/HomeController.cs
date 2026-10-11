using Microsoft.AspNetCore.Mvc;
using PROYECTO_MOVILES_PARTE_WEB.Filters;
using PROYECTO_MOVILES_PARTE_WEB.Models;
using PROYECTO_MOVILES_PARTE_WEB.Services;
using System.Globalization;

namespace PROYECTO_MOVILES_PARTE_WEB.Controllers
{
    [AdminFilter]
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int? idCancha, int? semana)
        {
            var vm = new DashboardViewModel();

            //Obtener API
            var usuarios = await _api.GetListAsync<Usuario>("UsuariosAPI/GetUsuarios");
            var alumnos = await _api.GetListAsync<Alumno>("AlumnosAPI/GetAlumnos");
            var reservas = await _api.GetListAsync<Reserva>("ReservasAPI/GetReservas");
            var canchas = await _api.GetListAsync<Cancha>("CanchasAPI/GetCanchas");
            var clases = await _api.GetListAsync<Clase>("ClasesAPI/GetClases");
            var clientes = await _api.GetListAsync<Cliente>("ClientesAPI/GetClientes");

            vm.Canchas = canchas;
            vm.TotalUsuarios = usuarios.Count > 0 ? usuarios.Count : clientes.Count;
            vm.TotalAlumnos = alumnos.Count;
            vm.TotalCanchas = canchas.Count;

            // Ganancia mensual
            var mesActual = DateTime.Now.Month;
            var anioActual = DateTime.Now.Year;
            vm.TotalGananciaMes = reservas
                .Where(r => r.HoraInicio.Month == mesActual && r.HoraInicio.Year == anioActual && r.EstadoReserva != "Cancelada")
                .Sum(r => r.MontoRecibido);

            if (vm.TotalGananciaMes == 0 && reservas.Count > 0)
            {
                vm.TotalGananciaMes = reservas.Where(r => r.EstadoReserva != "Cancelada").Sum(r => r.MontoRecibido);
            }

            var canchaSeleccionada = idCancha ?? canchas.FirstOrDefault()?.IdCancha ?? 1;
            vm.CanchaSeleccionadaId = canchaSeleccionada;

            // Calcular semana
            var hoy = DateTime.Now;
            var cal = CultureInfo.CurrentCulture.Calendar;
            var semanaActual = semana ?? cal.GetWeekOfYear(hoy, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            vm.SemanaNumero = semanaActual;
            vm.Anio = anioActual;

            var turnos = new List<TurnoSlot>();
            var nombresDias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

            var reservasCancha = reservas.Where(r => r.IdCancha == canchaSeleccionada && r.EstadoReserva != "Cancelada").ToList();
            var clasesCancha = clases.Where(c => c.IdCancha == canchaSeleccionada && c.EstadoClase != "Cancelada").ToList();

            for (int hora = 7; hora <= 22; hora++)
            {
                for (int dia = 1; dia <= 7; dia++)
                {
                    var slot = new TurnoSlot
                    {
                        DiaSemana = dia,
                        DiaNombre = nombresDias[dia - 1],
                        Hora = hora,
                        Tipo = "Libre"
                    };

                    var claseMatch = clasesCancha.FirstOrDefault(c =>
                    {
                        var diaClase = (int)c.FechaClase.DayOfWeek;
                        diaClase = diaClase == 0 ? 7 : diaClase;
                        return diaClase == dia && c.HoraInicio.Hour <= hora && c.HoraFin.Hour > hora;
                    });

                    if (claseMatch != null)
                    {
                        slot.Tipo = "Clase";
                        slot.Titulo = $"Clase #{claseMatch.IdClase} ({claseMatch.EstadoClase})";
                        slot.Estado = claseMatch.EstadoClase;
                        slot.IdReferencia = claseMatch.IdClase;
                    }
                    else
                    {
                        var reservaMatch = reservasCancha.FirstOrDefault(r =>
                        {
                            var diaReserva = (int)r.HoraInicio.DayOfWeek;
                            diaReserva = diaReserva == 0 ? 7 : diaReserva;
                            return diaReserva == dia && r.HoraInicio.Hour <= hora && r.HoraFin.Hour > hora;
                        });

                        if (reservaMatch != null)
                        {
                            slot.Tipo = "Reserva";
                            var cliente = clientes.FirstOrDefault(c => c.IdCliente == reservaMatch.IdCliente);
                            slot.Titulo = cliente != null ? cliente.NombreCliente : $"Reserva #{reservaMatch.IdReserva}";
                            slot.Estado = reservaMatch.EstadoReserva;
                            slot.IdReferencia = reservaMatch.IdReserva;
                        }
                    }

                    turnos.Add(slot);
                }
            }

            vm.Turnos = turnos;

            vm.Notificaciones.Add($"Tienes {reservasCancha.Count} reserva(s) registradas para esta cancha.");
            if (clasesCancha.Count > 0)
            {
                vm.Notificaciones.Add($"{clasesCancha.Count} clase(s) de academia programadas.");
            }

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
