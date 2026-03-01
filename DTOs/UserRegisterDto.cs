using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre 4 y 20 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}