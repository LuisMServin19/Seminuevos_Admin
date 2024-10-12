using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class ProximosPagosVerificacion
    {
        [Key]
        public int Id_unidad { get; set; }
        public string? Modelo { get; set; }
        public string? Marca { get; set; }
        public string? Sucursal { get; set; }

        [DisplayName("Fecha proxima verificaciones")]
        public DateTime Fech_prox_verificacion { get; set; }

        [DisplayName("Fecha pago verificaciones")]
        public DateTime Fecha_pago { get; set; }
        public bool MostrarBoton { get; set; } 

    }
}

