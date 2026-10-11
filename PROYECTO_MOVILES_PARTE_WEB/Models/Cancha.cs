namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Cancha
    {
        public int IdCancha { get; set; }
        public string NombreCancha { get; set; } = string.Empty;
        public string DescripcionCancha { get; set; } = string.Empty;
        public decimal PrecioAlquiler { get; set; }
        public bool ActivoCancha { get; set; } = true;
    }
}
