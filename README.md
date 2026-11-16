# ms1 – Verwaltung von Mitarbeitern

Teil von **M324 Projekt 4: Ticketsystem**. Team-Repo: [m324-ap23a-team3/team](https://github.com/m324-ap23a-team3/team)

- Stack: C# / ASP.NET Core Web API (.NET 10), Tests mit xUnit
- Pipeline: `.github/workflows/ci.yml` – Tests bei jedem Push und Pull Request, Image nach `ghcr.io/m324-ap23a-team3/ms1` bei Push auf `main`
- Rechte: Team `devops` (alle Mitglieder, Maintain). `main` ist geschützt: Pull Request mit 1 Approval und grünem Check `test`

## Lokal

```bash
dotnet test
dotnet run --project src/Ms1.Api
docker build -t ms1 . && docker run -p 8080:8080 ms1
```

Health-Check: `GET /health`
