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
}
