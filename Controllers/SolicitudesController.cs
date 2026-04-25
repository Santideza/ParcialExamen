using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParcialExamen.Models;
using ParcialExamen.Services;

namespace ParcialExamen.Controllers;

[Authorize]
public class SolicitudesController(SolicitudCreditoService service, UserManager<IdentityUser> userManager) : Controller
{
    public async Task<IActionResult> Index(FiltroSolicitudViewModel filtro)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            return View(filtro);
        }

        filtro.Resultados = await service.ObtenerPorUsuarioAsync(
            user.Id,
            filtro.Estado,
            filtro.MontoMin,
            filtro.MontoMax,
            filtro.FechaInicio,
            filtro.FechaFin);

        return View(filtro);
    }

    public IActionResult Crear()
    {
        return View(new RegistroSolicitudViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RegistroSolicitudViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cliente = await service.ObtenerClienteActivoPorUsuarioAsync(user.Id);
        if (cliente == null)
        {
            ModelState.AddModelError(string.Empty, "El cliente no existe o no esta activo.");
            return View(model);
        }

        if (await service.TieneSolicitudPendienteAsync(cliente.Id))
        {
            ModelState.AddModelError(string.Empty, "Ya tienes una solicitud pendiente.");
            return View(model);
        }

        if (model.MontoSolicitado > cliente.IngresosMensuales * 10)
        {
            ModelState.AddModelError(nameof(model.MontoSolicitado), "El monto no puede superar 10 veces tus ingresos mensuales.");
            return View(model);
        }

        await service.CrearSolicitudPendienteAsync(cliente, model.MontoSolicitado);
        TempData["SuccessMessage"] = "Solicitud registrada correctamente.";

        return RedirectToAction(nameof(Crear));
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var solicitud = await service.ObtenerPorIdYUsuarioAsync(id, user.Id);
        if (solicitud == null) return NotFound();

        HttpContext.Session.SetInt32("UltimaSolicitudId", solicitud.Id);
        HttpContext.Session.SetString("UltimaSolicitudMonto", solicitud.MontoSolicitado.ToString("C2"));

        return View(solicitud);
    }
}
