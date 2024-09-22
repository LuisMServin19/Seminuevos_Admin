using System.ComponentModel;

namespace WebApp.Models
{
    public class cat_branches
    {
        public int? ID_BRA { get; set; }
        public int? ID_DEA { get; set; }

        [DisplayName("Sucursal")]
        public string? BRA_NAME { get; set; }
        public string? BRA_CDATE { get; set; }
        public string? BRA_MAIN { get; set; }
        
        [DisplayName("Estatus")]
        public bool? BRA_ACTIVE { get; set; }

        public string Status
        {
            get
            {
                return BRA_ACTIVE == true ? "Activo" : "Desactivado";
            }
        }
    }
}