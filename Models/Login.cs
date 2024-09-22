using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Serfitex.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Usuario no valido")]
        [MaxLength(20)]
        [StringLength(20, MinimumLength = 3)]
        [DisplayName("Usuario")]
        [Column("Usuario")]
        public string? username { get; set; }

        [Required(ErrorMessage = "Contrase�a no valida")]
        [MaxLength(20)]
        [StringLength(20, MinimumLength = 5)]
        [DisplayName("Contrase�a")]
        public string? Contrasena { get; set; }
    }
}