
using System.ComponentModel;

namespace WebApp.Models
{
    public class login_customer
    {
        public int? ID_LOG { get; set; }
        public string? ID_CUS { get; set; }

        [DisplayName("Usuario")]
        public string? LOG_NICK { get; set; }

        [DisplayName("Contraseña")]
        public string? LOG_PASS { get; set; }

        [DisplayName("Nombre")]
        public string? LOG_NAME { get; set; }

        public string? LOG_CDATE { get; set; }

        [DisplayName("Estatus")]
        public bool LOG_ACTIVE { get; set; }

        public string Status
        {
            get
            {
                return LOG_ACTIVE == true ? "Activo" : "Desactivado";
            }
        }
    }
}