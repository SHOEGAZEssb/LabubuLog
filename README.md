# LabubuLog

A Razor Pages MVP for logging and rating games played together.

## Current MVP

- ASP.NET Core Razor Pages on .NET 8 LTS
- EF Core with SQLite
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

## Public Launch Notes

Before hosting this publicly, add authentication. The best next step is ASP.NET Core Identity with two accounts, then require authorization for all game pages. For hosted environments, set the connection string with `ConnectionStrings__LabubuLog` instead of storing production settings in `appsettings.json`.

The current metadata lookup uses Steam as a no-key MVP provider. For broader console and non-Steam coverage, add a second provider such as RAWG behind `IGameMetadataProvider` and configure its API key through environment variables.
