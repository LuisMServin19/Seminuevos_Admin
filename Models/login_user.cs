
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace WebApp.Models
{
    public class login_user
    {
        public int ID_USR { get; set; }
        public string? ID_ROL { get; set; }

        [DisplayName("Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        
        public string usr_nick { get; set; }

        [DisplayName("Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatorio.")]
        public string usr_pass { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]

        public string? usr_name { get; set; }

        [DisplayName("Perfil")]
        [Required(ErrorMessage = "El perfil es obligatorio.")]
        public string fiperfil { get; set; }

        [DisplayName("Perfil usuario")]
        public string tipo_perfil { get; set; }
        public DateTime fecha_alta { get; set; }



        [DisplayName("Estatus")]
        public bool? usr_active { get; set; }

        public string Status
        {
            get
            {
                return usr_active == true ? "Activo" : "Desactivado";
            }
        }
    }
}
