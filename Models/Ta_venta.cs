using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Ta_venta
    {
        [Key]

        public int Id_unidad { get; set; }

        [DisplayName("Fecha venta")]
        public DateTime? Fecha_venta { get; set; }
        public string? Vendedor { get; set; }
        public string? Comprador { get; set; }

    }
}

