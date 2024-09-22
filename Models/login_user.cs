
using System.ComponentModel;


namespace WebApp.Models
{
    public class login_user
    {
        public int? ID_USR { get; set; }
        public string? ID_ROL { get; set; }

        [DisplayName("Usuario")]
        public string? usr_nick { get; set; }

        [DisplayName("Contraseña")]
        public string? usr_pass { get; set; }

        [DisplayName("Nombre")]
        public string? usr_name { get; set; }

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
