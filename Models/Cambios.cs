using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NuevoBanorte.Models
{
    public class Cambios
    {
        [Key]

        public int Id { get; set; }

        public string Usuario { get; set; }

        public string Datos { get; set; }

        [DisplayName("Fecha de cambio")]
        public DateTime FechaCambios { get; set; }

    }
}
