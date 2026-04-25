using System.ComponentModel.DataAnnotations;

namespace ParcialExamen.Models;

public class Cliente
{
    public int Id { get; set; }
    
    [Required]
    public required string UsuarioId { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal IngresosMensuales { get; set; }
    
    public bool Activo { get; set; }
    
    public ICollection<SolicitudCredito> Solicitudes { get; set; } = [];
}
