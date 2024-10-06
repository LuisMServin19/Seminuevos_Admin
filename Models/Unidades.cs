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

        [DisplayName("Año")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Ano { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Color { get; set; }
        public IFormFile Imagen1 { get; set; }

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
        
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser un número")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public decimal Precio { get; set; }
        
        [Required(ErrorMessage = "Campo obligatorio")]
        public string? Sucursal { get; set; }
        public int? Estatus { get; set; }
        public string? EstatusTexto { get; set; }

        [DisplayName("Fecha ingreso")]
        public DateTime Fecha_ingreso { get; set; }

        [DisplayName("Proximo pago tenencia")]
        public DateTime? Fech_prox_tenecia { get; set; }

        [DisplayName("Proximo pago verificacion")]
        public DateTime? Fech_prox_verificacion { get; set; }

        [DisplayName("Fecha venta")]
        public DateTime? Fecha_venta { get; set; }
        public string? Vendedor { get; set; }
        public string? Comprador { get; set; }

    }
}

