namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public DateTime HoraInicio { get; set; } = DateTime.Now;
        public DateTime HoraFin { get; set; } = DateTime.Now.AddHours(1);
        public decimal PrecioReserva { get; set; }
        public string EstadoReserva { get; set; } = "Pendiente";
        public decimal MontoRecibido { get; set; }
        public int IdCliente { get; set; }
        public int IdCancha { get; set; }
        public int IdUsuario { get; set; }

        public string? NombreCliente { get; set; }
        public string? NombreCancha { get; set; }
        public string? NombreUsuario { get; set; }
    }
}
