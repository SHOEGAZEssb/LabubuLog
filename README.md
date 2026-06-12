# LabubuLog

A private Razor Pages app for logging and rating games played together.

## Current MVP

- ASP.NET Core Razor Pages on .NET 8 LTS
- EF Core with SQLite
- ASP.NET Core Identity with two seeded accounts
- Game library with search, status filter, and sorting
- Steam metadata lookup before manual entry
- Add/edit metadata, detail, and delete game pages
- Cover image and horizontal title image fields
- Dedicated rating page with separate Lebubu/Labubu scores plus shared average
- Dashboard and stats pages

## Run Locally

```powershell
dotnet restore
dotnet tool restore
dotnet run
```

The app creates `Data/labubulog.db` automatically from EF migrations.

In development, two users are seeded automatically:

- `lebubu` / `Lebubu123!`
- `labubu` / `Labubu123!`

Production does not use those fallback passwords. Set these environment variables before the first production start:

```text
SeedUsers__LebubuPassword
SeedUsers__LabubuPassword
```

## Free Azure Hosting

Use Azure App Service Free F1 for the app. `appsettings.Production.json` stores SQLite at `/home/data/labubulog.db`, which is the persistent home storage path on Azure App Service for Linux.

Required Azure app settings:

```text
ASPNETCORE_ENVIRONMENT=Production
SeedUsers__LebubuPassword=<strong password>
SeedUsers__LabubuPassword=<strong password>
```

Optional override if you move the database:

```text
ConnectionStrings__LabubuLog=Data Source=/home/data/labubulog.db
```

The current metadata lookup uses Steam as a no-key MVP provider. For broader console and non-Steam coverage, add a second provider such as RAWG behind `IGameMetadataProvider` and configure its API key through environment variables.
