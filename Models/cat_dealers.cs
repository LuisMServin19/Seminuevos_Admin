using System.ComponentModel;

namespace WebApp.Models
{
    public class cat_dealers
    {
        public int? ID_DEA { get; set; }
        public int? ID_CUS { get; set; }

        [DisplayName("Distribuidor")]
        public string? DEA_NAME { get; set; }
        public string? DEA_CODE { get; set; }
        public string? DEA_CDATE { get; set; }

        [DisplayName("Estatus")]
        public bool? DEA_ACTIVE { get; set; }
        public string Status
        {
            get
            {
                return DEA_ACTIVE == true ? "Activo" : "Desactivado";
            }
        }
    }
}