namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class PrestamoBalon
    {
        public int IdPrestamo { get; set; }
        public int? IdReserva { get; set; }
        public int? IdClase { get; set; }
        public DateTime FechaEntrega { get; set; } = DateTime.Now;
        public DateTime? FechaDevolucion { get; set; }
        public string EstadoPrestamo { get; set; } = "Pendiente";
    }
}
