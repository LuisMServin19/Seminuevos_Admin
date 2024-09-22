using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class TAMenu
    {
        [Key]

        public int fimenu { get; set; }

        //[Required(ErrorMessage = "Campo requerido")]
        [MaxLength(50)]
        [StringLength(50, MinimumLength = 5)]
        [DisplayName("Menu")]
        public string? fcdescmenu { get; set; }

        [DisplayName("Menú padre")]
        public int fipadre { get; set; }

        [DisplayName("Posición")]
        public int fiposicion { get; set; }

        //[Required(ErrorMessage = "Campo requerido")]
        [MaxLength(100)]
        [StringLength(100, MinimumLength = 5)]
        [DisplayName("Icono")]
        public string? fcicono { get; set; }

        [DisplayName("Estatus")]
        [Required(ErrorMessage = "Campo requerido")]
        public bool fbhabilitado { get; set; }

        //[Required(ErrorMessage = "Campo requerido")]
        [MaxLength(150)]
        [StringLength(150, MinimumLength = 5)]
        [DisplayName("URL")]
        public string? fcurl { get; set; }
    }
}