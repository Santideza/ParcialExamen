using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParcialExamen.Data;
using ParcialExamen.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<SolicitudCreditoService>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.Migrate();

    if (!await roleManager.RoleExistsAsync("Analista"))
    {
        await roleManager.CreateAsync(new IdentityRole("Analista"));
    }

    if (context.Clientes.Count() == 0)
    {
        var user1 = await userManager.FindByEmailAsync("cliente1@example.com");
        if (user1 == null)
        {
            user1 = new IdentityUser { UserName = "cliente1", Email = "cliente1@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user1, "Password123!");
        }

        var user2 = await userManager.FindByEmailAsync("cliente2@example.com");
        if (user2 == null)
        {
            user2 = new IdentityUser { UserName = "cliente2", Email = "cliente2@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(user2, "Password123!");
        }

        var analista = await userManager.FindByEmailAsync("analista@example.com");
        if (analista == null)
        {
            analista = new IdentityUser { UserName = "analista", Email = "analista@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(analista, "Password123!");
            await userManager.AddToRoleAsync(analista, "Analista");
        }

        var cliente1 = new ParcialExamen.Models.Cliente
        {
            UsuarioId = user1.Id,
            IngresosMensuales = 5000,
            Activo = true
        };

        var cliente2 = new ParcialExamen.Models.Cliente
        {
            UsuarioId = user2.Id,
            IngresosMensuales = 8000,
            Activo = true
        };

        context.Clientes.AddRange(cliente1, cliente2);
        context.SaveChanges();

        var solicitud1 = new ParcialExamen.Models.SolicitudCredito
        {
            ClienteId = cliente1.Id,
            MontoSolicitado = 10000,
            FechaSolicitud = DateTime.Now,
            Estado = ParcialExamen.Models.EstadoSolicitud.Pendiente
        };

        var solicitud2 = new ParcialExamen.Models.SolicitudCredito
        {
            ClienteId = cliente2.Id,
            MontoSolicitado = 20000,
            FechaSolicitud = DateTime.Now.AddDays(-1),
            Estado = ParcialExamen.Models.EstadoSolicitud.Aprobado
        };

        context.SolicitudesCredito.AddRange(solicitud1, solicitud2);
        context.SaveChanges();
    }
}

app.Run();
