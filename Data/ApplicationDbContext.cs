using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParcialExamen.Models;

namespace ParcialExamen.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<SolicitudCredito> SolicitudesCredito { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Solicitudes)
            .WithOne(s => s.Cliente)
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SolicitudCredito>()
            .ToTable(t => t.HasCheckConstraint("CK_MontoSolicitado_Positivo", "[MontoSolicitado] > 0"));

        modelBuilder.Entity<Cliente>()
            .ToTable(t => t.HasCheckConstraint("CK_IngresosMensuales_Positivo", "[IngresosMensuales] > 0"));
    }
}
