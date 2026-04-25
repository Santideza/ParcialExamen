using System.ComponentModel.DataAnnotations;

namespace ParcialExamen.Models;

public class FiltroSolicitudViewModel : IValidatableObject
{
    public EstadoSolicitud? Estado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto minimo no puede ser negativo.")]
    [Display(Name = "Monto minimo")]
    public decimal? MontoMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto maximo no puede ser negativo.")]
    [Display(Name = "Monto maximo")]
    public decimal? MontoMax { get; set; }

    [Display(Name = "Fecha inicio")]
    public DateTime? FechaInicio { get; set; }

    [Display(Name = "Fecha fin")]
    public DateTime? FechaFin { get; set; }

    public List<SolicitudCredito> Resultados { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MontoMin.HasValue && MontoMax.HasValue && MontoMin > MontoMax)
        {
            yield return new ValidationResult(
                "El monto minimo no puede ser mayor al monto maximo.",
                [nameof(MontoMin), nameof(MontoMax)]);
        }

        if (FechaInicio.HasValue && FechaFin.HasValue && FechaInicio > FechaFin)
        {
            yield return new ValidationResult(
                "La fecha de inicio no puede ser mayor a la fecha fin.",
                [nameof(FechaInicio), nameof(FechaFin)]);
        }
    }
}
