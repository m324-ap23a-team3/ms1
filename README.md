# ms1 – Verwaltung von Mitarbeitern

Teil von **M324 Projekt 4: Ticketsystem**. Team-Repo: [m324-ap23a-team3/team](https://github.com/m324-ap23a-team3/team)

- Stack: C# / ASP.NET Core Web API (.NET 10), EF Core mit MariaDB (Pomelo), Tests mit xUnit
- Pipeline: `.github/workflows/ci.yml` – Tests bei jedem Push und Pull Request, Image nach `ghcr.io/m324-ap23a-team3/ms1` bei Push auf `main`
- Rechte: Team `devops` (alle Mitglieder, Maintain). `main` ist geschützt: Pull Request mit 1 Approval und grünem Check `test`

## Lokal

Voraussetzungen: .NET 10 SDK, Docker.

```bash
docker compose up -d                  # lokale MariaDB auf Port 3306
dotnet run --project src/Ms1.Api      # wendet Migrationen beim Start an (Development)
dotnet test                           # braucht keine Datenbank
```

Health-Check: `GET /health`

## Datenbank

| | |
|---|---|
| DB | MariaDB 11.4, eigene Instanz nur für MS1 (`docker-compose.yml`) |
| Tabelle | `Mitarbeiter`: `Id` (auto), `Vorname`, `Nachname`, `Beitrittsdatum` (date), `Skilllevel` (1–5, Check-Constraint) |
| Connection-String | nur über Konfiguration: `ConnectionStrings:Ms1Db` (lokal in `appsettings.Development.json`, sonst Umgebungsvariable `ConnectionStrings__Ms1Db`) |
| Migrationen | `src/Ms1.Api/Data/Migrations`. Beim Start angewendet, wenn `Datenbank:MigrationBeimStart=true` |

Neue Migration erstellen:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/Ms1.Api --output-dir Data/Migrations
```

Das Passwort in `docker-compose.yml` und `appsettings.Development.json` gilt nur für die lokale Entwicklungs-DB. Eigene Werte lassen sich über eine `.env`-Datei (`MS1_DB_PASSWORT`, `MS1_DB_PORT`) setzen; die Datei ist in `.gitignore`.

## Architektur

Endpoints → Service → `IMitarbeiterRepository` → EF Core/MariaDB. Die Tests ersetzen das Repository durch `FakeMitarbeiterRepository` und laufen ohne Datenbank. Beschreibung: [team/docs/Applikation.md](https://github.com/m324-ap23a-team3/team/blob/main/docs/Applikation.md#ms1-testbare-architektur)

## Tests

`dotnet test` – alle Tests laufen ohne Datenbank (Repository durch `FakeMitarbeiterRepository` ersetzt). Jeder Test ist im Code mit `/// <summary>` beschrieben (Was wird geprüft, Happy oder Sad Path, welches Abnahmekriterium).

| Datei | Ebene | Inhalt |
|---|---|---|
| `tests/Ms1.Api.Tests/US1/MitarbeiterErfassenServiceTests.cs` | Unit (Service) | US1: gültiger Mitarbeiter, Grenzwerte, Datumsformate, Skilllevel 0/6, fehlende Felder, DB-Fehler |
| `tests/Ms1.Api.Tests/US1/MitarbeiterErfassenApiTests.cs` | API (HTTP) | US1: 201 mit Mitarbeiter, 400 mit Meldung pro Feld, ungültiges JSON, 503 bei DB-Fehler |
| `tests/Ms1.Api.Tests/US2/MitarbeiterAuslesenServiceTests.cs` | Unit (Service) | US2: Liste mit Einträgen, leere Liste mit Meldung, Fehler beim Laden, 10'000 Einträge |
| `tests/Ms1.Api.Tests/US2/MitarbeiterAuslesenApiTests.cs` | API (HTTP) | US2: erfasster Mitarbeiter erscheint in der Übersicht, leere Liste, JSON-Format für MS2, 503, 405 |
| `tests/Ms1.Api.Tests/US2/MitarbeiterRepositoryTests.cs` | Repository (EF InMemory) | US2 AK4: kein Change-Tracking, Sortierung nach ID |
