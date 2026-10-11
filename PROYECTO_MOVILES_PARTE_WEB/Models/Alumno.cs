namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Alumno
    {
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = string.Empty;
        public string? TelefonoApoderado { get; set; }
        public DateOnly? FechaIngreso { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public bool ActivoAlumno { get; set; } = true;
        public int IdUsuario { get; set; }

        public string? NombreProfesor { get; set; }
    }
}
