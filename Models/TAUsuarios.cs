using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class TAUsuarios
    {
        [Key]

        [DisplayName("ID")]
        public int fiUsr { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(100)]
        [StringLength(100, MinimumLength = 3)]
        [DisplayName("Nombre")]
        public string? fcNombre { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(20)]
        [StringLength(20, MinimumLength = 5)]
        [DisplayName("Clave")]
        public string? fcClave { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(400)]
        [StringLength(400, MinimumLength = 5)]
        [DisplayName("Contraseña")]
        public string? fcPassword { get; set; }

        [DisplayName("Estatus")]
        public int fiStatus { get; set; }

        [DisplayName("Perfil")]
        public int fiPerfil { get; set; }

        [DisplayName("Fecha de Alta")]
        public DateTime fdfechaalta { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(50)]
        [StringLength(50, MinimumLength = 5)]
        [DisplayName("Correo")]
        public string? fcMail { get; set; }

        [DisplayName("Intentos")]
        public int fintentos { get; set; }

        [DisplayName("Linea")]
        public int filinea { get; set; }

        [DisplayName("Fecha Última Conexión ")]
        public DateTime fdfechaultima_coneccion { get; set; }

        [DisplayName("Cliente")]
        public int IdCliente { get; set; }

    }
}