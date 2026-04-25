using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcialExamen.Models;
using ParcialExamen.Services;

namespace ParcialExamen.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController(SolicitudCreditoService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var solicitudes = await service.ObtenerPendientesAsync();
        return View(solicitudes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await service.ObtenerPorIdAsync(id);
        if (solicitud == null) return NotFound();

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["ErrorMessage"] = "La solicitud ya fue procesada.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.Cliente == null || solicitud.MontoSolicitado > solicitud.Cliente.IngresosMensuales * 5)
        {
            TempData["ErrorMessage"] = "No se puede aprobar: el monto excede 5 veces los ingresos.";
            return RedirectToAction(nameof(Index));
        }

        await service.ActualizarEstadoAsync(id, EstadoSolicitud.Aprobado);
        TempData["SuccessMessage"] = "Solicitud aprobada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string? motivoRechazo)
    {
        var solicitud = await service.ObtenerPorIdAsync(id);
        if (solicitud == null) return NotFound();

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["ErrorMessage"] = "La solicitud ya fue procesada.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(motivoRechazo))
        {
            TempData["ErrorMessage"] = "Debe ingresar el motivo de rechazo.";
            return RedirectToAction(nameof(Index));
        }

        await service.ActualizarEstadoAsync(id, EstadoSolicitud.Rechazado, motivoRechazo);
        TempData["SuccessMessage"] = "Solicitud rechazada correctamente.";

        return RedirectToAction(nameof(Index));
    }
}
