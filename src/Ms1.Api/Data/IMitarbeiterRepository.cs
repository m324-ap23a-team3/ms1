using Ms1.Api.Models;

namespace Ms1.Api.Data;

/// <summary>
/// Zugriff auf die gespeicherten Mitarbeiter. Die Service-Schicht kennt nur dieses Interface,
/// nicht EF Core. In den Unit Tests ersetzt eine Fake-Implementierung die Datenbank.
/// </summary>
public interface IMitarbeiterRepository
{
    /// <summary>Speichert einen neuen Mitarbeiter und gibt ihn mit der vergebenen ID zurück.</summary>
    Task<Mitarbeiter> HinzufuegenAsync(Mitarbeiter mitarbeiter, CancellationToken cancellationToken = default);
}
