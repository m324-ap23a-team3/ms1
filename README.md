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
