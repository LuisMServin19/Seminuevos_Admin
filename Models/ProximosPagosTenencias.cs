using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class ProximosPagosTenencias
    {
        [Key]

        public int Id_unidad { get; set; }
        public string? Modelo { get; set; }
        public string? Marca { get; set; }
        public string? Sucursal { get; set; }

        [DisplayName("Fecha proxima tenencia")]
        public DateTime Fech_prox_tenecia { get; set; }

        [DisplayName("Fecha pago tenencia")]
        public DateTime Fecha_pago { get; set; }

    }
}

