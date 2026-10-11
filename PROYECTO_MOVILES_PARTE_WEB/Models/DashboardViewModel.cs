namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class DashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalAlumnos { get; set; }
        public decimal TotalGananciaMes { get; set; }
        public int TotalCanchas { get; set; }

        public int CanchaSeleccionadaId { get; set; }
        public int SemanaNumero { get; set; }
        public int Anio { get; set; }

        public List<Cancha> Canchas { get; set; } = new();
        public List<TurnoSlot> Turnos { get; set; } = new();
        public List<string> Notificaciones { get; set; } = new();
    }

    public class TurnoSlot
    {
        public int DiaSemana { get; set; }
        public string DiaNombre { get; set; } = string.Empty;
        public int Hora { get; set; }
        public string HoraTexto => $"{Hora:D2}:00";
        public string Tipo { get; set; } = "Libre";
        public string Titulo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IdReferencia { get; set; }
    }
}
