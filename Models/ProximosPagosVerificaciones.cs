using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class ProximosPagosVerificaciones
    {
        [Key]

        public int Id_unidad { get; set; }

        public string? Modelo { get; set; }

        public string? Marca { get; set; }

        [DisplayName("Número de serie")]
        public string? Num_serie { get; set; }

        [DisplayName("Año")]
        public string? Ano { get; set; }
        public string? Color { get; set; }

        [DisplayName("Fecha factura")]
        public DateTime Fecha_factura { get; set; }

        [DisplayName("Fecha tenencia")]
        public DateTime Fecha_tenencia { get; set; }
        
        public string? Seguro { get; set; }
        public string? Comentario { get; set; }
        public string? Sucursal { get; set; }
        public int? Estatus { get; set; }
        public string? EstatusTexto { get; set; }
        
        [DisplayName("Fecha ingreso")]
        public DateTime Fecha_ingreso { get; set; }

        [DisplayName("Proximo pago tenencia")]
        public DateTime? Fech_prox_tenecia { get; set; }

        [DisplayName("Proximo pago verificacion")]
        public DateTime? Fech_prox_verificacion { get; set; }

    }
}

