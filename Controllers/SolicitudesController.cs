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

    public async Task<IActionResult> Detalle(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var solicitud = await service.ObtenerPorIdYUsuarioAsync(id, user.Id);
        if (solicitud == null) return NotFound();

        return View(solicitud);
    }
}
