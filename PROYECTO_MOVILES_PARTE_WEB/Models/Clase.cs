namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Clase
    {
        public int IdClase { get; set; }
        public int IdUsuario { get; set; }
        public int IdCancha { get; set; }
        public DateOnly FechaClase { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly HoraInicio { get; set; } = new TimeOnly(15, 0);
        public TimeOnly HoraFin { get; set; } = new TimeOnly(17, 0);
        public string EstadoClase { get; set; } = "Programada";
        public bool ActivoClase { get; set; } = true;

        public string? NombreProfesor { get; set; }
        public string? NombreCancha { get; set; }
    }
}
