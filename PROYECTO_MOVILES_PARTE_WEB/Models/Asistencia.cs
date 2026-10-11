namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Asistencia
    {
        public int IdAsistencia { get; set; }
        public int IdClase { get; set; }
        public int IdAlumno { get; set; }
        public string EstadoAsistencia { get; set; } = "Presente";

        public string? NombreAlumno { get; set; }
        public string? TelefonoApoderado { get; set; }
    }

    public class AsistenciaClaseViewModel
    {
        public Clase Clase { get; set; } = new Clase();
        public List<AsistenciaItemViewModel> Alumnos { get; set; } = new List<AsistenciaItemViewModel>();
    }

    public class AsistenciaItemViewModel
    {
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = string.Empty;
        public string? TelefonoApoderado { get; set; }
        public int? IdAsistencia { get; set; }
        public string EstadoAsistencia { get; set; } = "Presente";
    }
}
