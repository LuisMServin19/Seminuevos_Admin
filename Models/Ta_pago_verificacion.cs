using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Ta_pago_verificacion
    {
        [Key]

        public int Id_unidad { get; set; }

        public string Modelo { get; set; }

        [DisplayName("Fecha tenencia")]
        public DateTime Fecha_verificacion { get; set; }

        [DisplayName("Fecha pago")]
        public DateTime Fecha_pago { get; set; }

        [DisplayName("Fecha pago")]
        public DateTime? Fech_prox_verificacion { get; set; }

    }
}

