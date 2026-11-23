using Microsoft.EntityFrameworkCore;
using Ms1.Api.Models;

namespace Ms1.Api.Data;

/// <summary>
/// Implementierung von <see cref="IMitarbeiterRepository"/> mit EF Core und MariaDB.
/// Enthält keine Geschäftslogik, nur Datenzugriff.
/// </summary>
public class MitarbeiterRepository(Ms1DbContext db) : IMitarbeiterRepository
{
    public async Task<Mitarbeiter> HinzufuegenAsync(Mitarbeiter mitarbeiter, CancellationToken cancellationToken = default)
    {
        db.Mitarbeiter.Add(mitarbeiter);
        await db.SaveChangesAsync(cancellationToken);
        return mitarbeiter;
    }

    public async Task<IReadOnlyList<Mitarbeiter>> AlleLesenAsync(CancellationToken cancellationToken = default)
    {
        // US2 AK4: AsNoTracking, weil die Daten nur gelesen werden. EF Core legt so keine
        // Tracking-Einträge an, was bei vielen Mitarbeitern Zeit und Speicher spart.
        return await db.Mitarbeiter
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }
}
