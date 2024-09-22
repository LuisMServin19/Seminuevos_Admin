using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class TAPerfilMenu
    {
        [Key]

        public int Id { get; set; }

        [DisplayName("Perfil")]
        public int fiPerfil { get; set; }

        [DisplayName("Menú")]
        public int? fiMenu { get; set; }

        [DisplayName("Usuario")]
        public int? idUsuario { get; set; }

        [DisplayName("Fecha de Alta")]
        public DateTime? fdFechaAlta { get; set; }

    }
}