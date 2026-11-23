using Ms1.Api.Data;
using Ms1.Api.Models;

namespace Ms1.Api.Tests.Fakes;

/// <summary>
/// Ersetzt die Datenbank in den Tests: hält die Mitarbeiter in einer Liste und vergibt IDs selbst.
/// Mit <see cref="Fehler"/> lässt sich ein Datenbankfehler simulieren.
/// </summary>
public class FakeMitarbeiterRepository : IMitarbeiterRepository
{
    private readonly List<Mitarbeiter> _mitarbeiter = [];
    private readonly Lock _lock = new();
    private int _naechsteId = 1;

    /// <summary>Wenn gesetzt, wirft jeder Zugriff diese Exception (z. B. «Datenbank nicht erreichbar»).</summary>
    public Exception? Fehler { get; set; }

    /// <summary>Alle bisher gespeicherten Mitarbeiter.</summary>
    public IReadOnlyList<Mitarbeiter> Gespeichert
    {
        get { lock (_lock) { return [.. _mitarbeiter]; } }
    }

    /// <summary>Setzt Daten und Fehler zurück, damit jeder Test mit einem leeren Zustand startet.</summary>
    public void Zuruecksetzen()
    {
        lock (_lock)
        {
            _mitarbeiter.Clear();
            _naechsteId = 1;
            Fehler = null;
        }
    }

    public Task<Mitarbeiter> HinzufuegenAsync(Mitarbeiter mitarbeiter, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (Fehler is not null)
            {
                throw Fehler;
            }

            mitarbeiter.Id = _naechsteId++;
            _mitarbeiter.Add(mitarbeiter);
            return Task.FromResult(mitarbeiter);
        }
    }
}
