using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParcialExamen.Data;
using ParcialExamen.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<SolicitudCreditoService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisUrl = builder.Configuration["Redis:ConnectionString"]
        ?? builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
    var redisUri = new Uri(redisUrl);
    var userInfo = redisUri.UserInfo.Split(':', 2);

    options.Configuration = $"{redisUri.Host}:{redisUri.Port},user={userInfo[0]},password={userInfo[1]},abortConnect=false";
    options.InstanceName = "ParcialExamen:";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseSession();

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

    var user1 = await EnsureUserAsync(context, userManager, "cliente1@example.com", "Password123!");
    var user2 = await EnsureUserAsync(context, userManager, "cliente2@example.com", "Password123!");
    var analista = await EnsureUserAsync(context, userManager, "analista@example.com", "Password123!");

    if (!await userManager.IsInRoleAsync(analista, "Analista"))
    {
        EnsureSucceeded(await userManager.AddToRoleAsync(analista, "Analista"), "asignar el rol Analista");
    }

    var cliente1 = await EnsureClienteAsync(context, user1, "Pedro", 5000);
    var cliente2 = await EnsureClienteAsync(context, user2, "Maria", 8000);

    if (!context.SolicitudesCredito.Any())
    {
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

static async Task<IdentityUser> EnsureUserAsync(
    ApplicationDbContext context,
    UserManager<IdentityUser> userManager,
    string email,
    string password)
{
    var normalizedEmail = userManager.NormalizeEmail(email);
    var normalizedUserName = userManager.NormalizeName(email);
    var matchingUsers = await context.Users
        .Where(user => user.NormalizedEmail == normalizedEmail)
        .OrderByDescending(user => user.NormalizedUserName == normalizedUserName)
        .ThenBy(user => user.Id)
        .ToListAsync();

    var user = matchingUsers.FirstOrDefault();

    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        EnsureSucceeded(await userManager.CreateAsync(user, password), $"crear el usuario {email}");
        return user;
    }

    var changed = false;

    if (user.UserName != email)
    {
        user.UserName = email;
        user.NormalizedUserName = normalizedUserName;
        changed = true;
    }

    if (user.Email != email)
    {
        user.Email = email;
        user.NormalizedEmail = normalizedEmail;
        changed = true;
    }

    if (!user.EmailConfirmed)
    {
        user.EmailConfirmed = true;
        changed = true;
    }

    if (changed)
    {
        EnsureSucceeded(await userManager.UpdateAsync(user), $"actualizar el usuario {email}");
    }

    if (!await userManager.CheckPasswordAsync(user, password))
    {
        if (await userManager.HasPasswordAsync(user))
        {
            EnsureSucceeded(await userManager.RemovePasswordAsync(user), $"remover el password anterior de {email}");
        }

        EnsureSucceeded(await userManager.AddPasswordAsync(user, password), $"asignar el password de {email}");
    }

    return user;
}

static async Task<ParcialExamen.Models.Cliente> EnsureClienteAsync(
    ApplicationDbContext context,
    IdentityUser user,
    string nombre,
    decimal ingresosMensuales)
{
    var cliente = await context.Clientes.FirstOrDefaultAsync(cliente => cliente.UsuarioId == user.Id);

    if (cliente == null)
    {
        var matchingUserIds = await context.Users
            .Where(existingUser => existingUser.NormalizedEmail == user.NormalizedEmail)
            .Select(existingUser => existingUser.Id)
            .ToListAsync();

        cliente = await context.Clientes
            .FirstOrDefaultAsync(cliente => matchingUserIds.Contains(cliente.UsuarioId));
    }

    if (cliente == null)
    {
        cliente = new ParcialExamen.Models.Cliente
        {
            UsuarioId = user.Id,
            Nombre = nombre,
            IngresosMensuales = ingresosMensuales,
            Activo = true
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    var changed = false;

    if (cliente.UsuarioId != user.Id)
    {
        cliente.UsuarioId = user.Id;
        changed = true;
    }

    if (cliente.Nombre != nombre)
    {
        cliente.Nombre = nombre;
        changed = true;
    }

    if (cliente.IngresosMensuales != ingresosMensuales)
    {
        cliente.IngresosMensuales = ingresosMensuales;
        changed = true;
    }

    if (!cliente.Activo)
    {
        cliente.Activo = true;
        changed = true;
    }

    if (changed)
    {
        await context.SaveChangesAsync();
    }

    return cliente;
}

static void EnsureSucceeded(IdentityResult result, string action)
{
    if (result.Succeeded)
    {
        return;
    }

    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
    throw new InvalidOperationException($"No se pudo {action}: {errors}");
}
