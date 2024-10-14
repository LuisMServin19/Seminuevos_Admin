using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Unidades
    {
        [Key]

        public int Id_unidad { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Modelo { get; set; }
        public string? Version { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Tipo { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Marca { get; set; }
        public string? Transmision { get; set; }

        [DisplayName("Número de placa")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Num_placa { get; set; }

        [DisplayName("Número de serie")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Num_serie { get; set; }

        [Required(ErrorMessage = "El campo Año es obligatorio.")]

        [DisplayName("Año")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Color { get; set; }

        [DisplayName("Fecha factura")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime Fecha_factura { get; set; }

        [DisplayName("Tipo de factura")]
        public string? Tipo_factura { get; set; }


        [DisplayName("Fecha pago ultima tenencia")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime Fecha_tenencia { get; set; }

        [DisplayName("Fecha pago ultima verificacion")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime Fecha_verificacion { get; set; }
        public string? Seguro { get; set; }
        public string? Aseguradora { get; set; }

        [DisplayName("Duplicado de llaves")]
        public string? Duplicado_llave { get; set; }
        public string? Comentario { get; set; }
        
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
        
        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Sucursal { get; set; }
        public int? Estatus { get; set; }
        public string? EstatusTexto { get; set; }

        [DisplayName("Fecha de ingreso al lote")]
        public DateTime Fecha_ingreso { get; set; }

        [DisplayName("Proximo pago tenencia")]
        public DateTime? Fech_prox_tenecia { get; set; }

        [DisplayName("Proximo pago verificacion")]
        public DateTime? Fech_prox_verificacion { get; set; }

        [DisplayName("Fecha venta")]
        public DateTime? Fecha_venta { get; set; }
        public string? Vendedor { get; set; }
        public string? Comprador { get; set; }
        public int? Celular { get; set; }
        public int? Tel_casa { get; set; }
        public int? Tel_oficina { get; set; }
        public string? Correo { get; set; }
        public decimal Gasto { get; set; }
        public string? Concepto { get; set; }
        [DisplayName("Fecha Gasto")]
        public DateTime Fecha_gasto { get; set; }

    }
}

