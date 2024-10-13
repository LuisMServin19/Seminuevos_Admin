using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Ta_gastos
    {
        [Key]

        public int Id_unidad { get; set; }

        [DisplayName("Gastos")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Gastos { get; set; }
        public string? Concepto { get; set; }
        public decimal Fecha_gasto { get; set; }
    }
}

