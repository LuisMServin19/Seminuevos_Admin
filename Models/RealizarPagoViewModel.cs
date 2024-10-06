using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Serfitex.Models
{
    public class RealizarPagoViewModel
    {
        public Unidades Unidad { get; set; }
    public Ta_pago_tenencia PagoTenencia { get; set; }

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La fecha de pago es requerida.")]
    public DateTime? Fecha_pago { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Fech_prox_tenecia { get; set; }
    }

}

