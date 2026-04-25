using Microsoft.EntityFrameworkCore;
using ParcialExamen.Data;
using ParcialExamen.Models;

namespace ParcialExamen.Services;

public class SolicitudCreditoService
{
    private readonly ApplicationDbContext _context;

    public SolicitudCreditoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SolicitudCredito>> ObtenerPorUsuarioAsync(
        string usuarioId,
        EstadoSolicitud? estado,
        decimal? montoMin,
        decimal? montoMax,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var query = _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Cliente!.UsuarioId == usuarioId)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(s => s.Estado == estado.Value);

        if (montoMin.HasValue)
            query = query.Where(s => s.MontoSolicitado >= montoMin.Value);

        if (montoMax.HasValue)
            query = query.Where(s => s.MontoSolicitado <= montoMax.Value);

        if (fechaInicio.HasValue)
            query = query.Where(s => s.FechaSolicitud >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(s => s.FechaSolicitud <= fechaFin.Value.AddDays(1).AddTicks(-1));

        return await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();
    }

    public async Task<SolicitudCredito?> ObtenerPorIdYUsuarioAsync(int id, string usuarioId)
    {
        return await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id && s.Cliente!.UsuarioId == usuarioId);
    }
}