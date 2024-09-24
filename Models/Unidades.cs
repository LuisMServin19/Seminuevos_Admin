using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class Unidades
    {
        [Key]

        public int Id_unidad { get; set; }

        public string Modelo { get; set; }

        public string Marca { get; set; }
        public string Num_serie { get; set; }
        public string Ano { get; set; }

        [DisplayName("Fecha factura")]
        public DateTime Fecha_factura { get; set; }

        [DisplayName("Fecha tenencia")]
        public DateTime Fecha_tenencia { get; set; }
        
        public string Seguro { get; set; }
        public string Comentario { get; set; }
        public bool Estatus { get; set; }

        [DisplayName("Fecha ingreso")]
        public DateTime Fecha_ingreso { get; set; }

    }
}

