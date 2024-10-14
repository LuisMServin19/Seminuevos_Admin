using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Ta_gastos
    {
        [Key]

        public int? Id_unidad { get; set; }

        [DisplayName("Precio compra")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Precio_compra { get; set; }

        [DisplayName("Gastos")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Gastos { get; set; }

        [DisplayName("Total compra mas gastos")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Total_compra_gastos { get; set; }

        [DisplayName("Precio venta")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Precio { get; set; }
        public string? Concepto { get; set; }
        public decimal Fecha_gasto { get; set; }

        public string? Modelo { get; set; }
        public string? Sucursal { get; set; }

    }
}

