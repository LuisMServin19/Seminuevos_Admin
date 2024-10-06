using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Ta_pago_tenencia
    {
        [Key]

        public int Id_unidad { get; set; }

        public string? Modelo { get; set; }

        [DisplayName("Fecha tenencia")]
        public DateTime? Fecha_tenencia { get; set; }

        [DisplayName("Fecha pago")]
        public DateTime? Fecha_pago { get; set; }

        [DisplayName("Fecha pago")]
        public DateTime? Fech_prox_tenecia { get; set; }

    }
}

