using ParcialExamen.Data;
using ParcialExamen.Models;

namespace ParcialExamen.Services;

public class SolicitudCreditoService(ApplicationDbContext context)
{
    public async Task<(bool isValid, string? errorMessage)> ValidarSolicitudAsync(SolicitudCredito solicitud)
    {
        var cliente = await context.Clientes.FindAsync(solicitud.ClienteId);
        if (cliente == null)
            return (false, "Cliente no encontrado");

        var solicitudPendiente = context.SolicitudesCredito
            .Where(s => s.ClienteId == solicitud.ClienteId && s.Estado == EstadoSolicitud.Pendiente)
            .FirstOrDefault();

        if (solicitudPendiente != null && solicitudPendiente.Id != solicitud.Id)
            return (false, "El cliente ya tiene una solicitud en estado Pendiente");

        if (solicitud.MontoSolicitado > cliente.IngresosMensuales * 5)
            return (false, "El monto solicitado no puede ser mayor a 5 veces los ingresos mensuales");

        return (true, null);
    }
}
