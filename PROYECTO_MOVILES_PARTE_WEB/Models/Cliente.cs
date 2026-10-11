namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = "Particular";
        public bool ActivoCliente { get; set; } = true;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? TelefonoCliente { get; set; }
    }
}
