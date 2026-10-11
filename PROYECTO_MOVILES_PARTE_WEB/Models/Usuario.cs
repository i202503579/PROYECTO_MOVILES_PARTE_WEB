namespace PROYECTO_MOVILES_PARTE_WEB.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string UsernameUsuario { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool ActivoUsuario { get; set; } = true;
        public string Rol { get; set; } = string.Empty;
    }
}
