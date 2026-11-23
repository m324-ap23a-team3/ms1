using Microsoft.EntityFrameworkCore;
using Ms1.Api.Models;

namespace Ms1.Api.Data;

/// <summary>
/// Datenbank von MS1. Nur MS1 greift darauf zu; MS2 liest Mitarbeiter über den Endpunkt aus US2.
/// </summary>
public class Ms1DbContext(DbContextOptions<Ms1DbContext> options) : DbContext(options)
{
    public DbSet<Mitarbeiter> Mitarbeiter => Set<Mitarbeiter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mitarbeiter>(entity =>
        {
            entity.ToTable("Mitarbeiter", table => table.HasCheckConstraint(
                "CK_Mitarbeiter_Skilllevel",
                $"`Skilllevel` BETWEEN {Models.Mitarbeiter.SkilllevelMin} AND {Models.Mitarbeiter.SkilllevelMax}"));

            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedOnAdd();
            entity.Property(m => m.Vorname).IsRequired().HasMaxLength(Models.Mitarbeiter.NameMaxLaenge);
            entity.Property(m => m.Nachname).IsRequired().HasMaxLength(Models.Mitarbeiter.NameMaxLaenge);
            entity.Property(m => m.Beitrittsdatum).IsRequired();
            entity.Property(m => m.Skilllevel).IsRequired();
        });
    }
}
