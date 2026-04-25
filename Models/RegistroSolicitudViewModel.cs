using System.ComponentModel.DataAnnotations;

namespace ParcialExamen.Models;

public class RegistroSolicitudViewModel
{
    [Display(Name = "Monto solicitado")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Ingrese un monto valido.")]
    public decimal MontoSolicitado { get; set; }
}
