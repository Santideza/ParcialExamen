using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ParcialExamen.Data;
using ParcialExamen.Models;
using System.Text.Json;

namespace ParcialExamen.Services;

public class SolicitudCreditoService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public SolicitudCreditoService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<SolicitudCredito>> ObtenerPorUsuarioAsync(
        string usuarioId,
        EstadoSolicitud? estado,
        decimal? montoMin,
        decimal? montoMax,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var version = await ObtenerVersionCacheUsuarioAsync(usuarioId);
        var cacheKey = $"solicitudes:{usuarioId}:{version}:{estado}:{montoMin}:{montoMax}:{fechaInicio:yyyyMMdd}:{fechaFin:yyyyMMdd}";
        var solicitudesCache = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrWhiteSpace(solicitudesCache))
        {
            return JsonSerializer.Deserialize<List<SolicitudCredito>>(solicitudesCache, JsonOptions) ?? [];
        }

        var query = _context.SolicitudesCredito
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

        var solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(solicitudes, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

        return solicitudes;
    }

    public async Task<SolicitudCredito?> ObtenerPorIdYUsuarioAsync(int id, string usuarioId)
    {
        return await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id && s.Cliente!.UsuarioId == usuarioId);
    }

    public async Task<List<SolicitudCredito>> ObtenerPendientesAsync()
    {
        return await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<SolicitudCredito?> ObtenerPorIdAsync(int id)
    {
        return await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Cliente?> ObtenerClienteActivoPorUsuarioAsync(string usuarioId)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Activo);
    }

    public async Task<bool> TieneSolicitudPendienteAsync(int clienteId)
    {
        return await _context.SolicitudesCredito
            .AnyAsync(s => s.ClienteId == clienteId && s.Estado == EstadoSolicitud.Pendiente);
    }

    public async Task CrearSolicitudPendienteAsync(Cliente cliente, decimal montoSolicitado)
    {
        var solicitud = new SolicitudCredito
        {
            ClienteId = cliente.Id,
            MontoSolicitado = montoSolicitado,
            FechaSolicitud = DateTime.Now,
            Estado = EstadoSolicitud.Pendiente
        };

        _context.SolicitudesCredito.Add(solicitud);
        await _context.SaveChangesAsync();
        await InvalidarCacheUsuarioAsync(cliente.UsuarioId);
    }

    public async Task ActualizarEstadoAsync(int solicitudId, EstadoSolicitud estado, string? motivoRechazo = null)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);

        if (solicitud == null)
        {
            return;
        }

        solicitud.Estado = estado;
        solicitud.MotivoRechazo = motivoRechazo;
        await _context.SaveChangesAsync();

        if (solicitud.Cliente != null)
        {
            await InvalidarCacheUsuarioAsync(solicitud.Cliente.UsuarioId);
        }
    }

    private async Task<string> ObtenerVersionCacheUsuarioAsync(string usuarioId)
    {
        var versionKey = ObtenerVersionKey(usuarioId);
        var version = await _cache.GetStringAsync(versionKey);

        if (!string.IsNullOrWhiteSpace(version))
        {
            return version;
        }

        version = Guid.NewGuid().ToString("N");
        await _cache.SetStringAsync(versionKey, version);
        return version;
    }

    private async Task InvalidarCacheUsuarioAsync(string usuarioId)
    {
        await _cache.SetStringAsync(ObtenerVersionKey(usuarioId), Guid.NewGuid().ToString("N"));
    }

    private static string ObtenerVersionKey(string usuarioId)
    {
        return $"solicitudes:{usuarioId}:version";
    }
}
