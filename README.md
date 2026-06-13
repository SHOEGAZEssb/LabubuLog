# LabubuLog

A private Razor Pages app for logging and rating games played together.

## Current MVP

- ASP.NET Core Razor Pages on .NET 8 LTS
- EF Core with SQLite locally and SQL Server support for MonsterASP.NET
- ASP.NET Core Identity with two seeded accounts
- Game library with search, status filter, and sorting
- Steam metadata lookup plus optional RAWG lookup before manual entry
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

## MonsterASP.NET Hosting

MonsterASP.NET is the current free hosting target for the MVP. Production is configured to use SQL Server because MonsterASP.NET includes managed MSSQL/MySQL databases and supports ASP.NET Core/.NET 8.

Create a website and an MSSQL database in the MonsterASP.NET control panel. Then add these environment variables under:

```text
Websites -> Manage website -> Scripting -> Environment Variables
```

Required production variables:

```text
ASPNETCORE_ENVIRONMENT=Production
DatabaseProvider=SqlServer
ConnectionStrings__LabubuLog=<MonsterASP MSSQL connection string>
SeedUsers__LebubuPassword=<strong password>
SeedUsers__LabubuPassword=<strong password>
GameMetadata__RawgApiKey=<RAWG API key>
```

The first production start creates the SQL Server schema automatically, then seeds the two private users with the passwords above. Keep these values out of source control.

### Continuous Deployment With GitHub Actions

Activate WebDeploy for the site in the MonsterASP.NET control panel. Add these GitHub repository secrets from the WebDeploy details:

```text
MONSTERASP_WEBSITE_NAME=siteXXXXX
MONSTERASP_SERVER_COMPUTER_NAME=https://siteXXXXX.siteasp.net:8172
MONSTERASP_USERNAME=siteXXXXX
MONSTERASP_PASSWORD=<webdeploy password>
```

The `Build and deploy to MonsterASP.NET` workflow deploys automatically after every push to `main`. Pull requests only build and publish-check the app; they do not deploy. You can also run the workflow manually from the GitHub Actions tab.

### Deploy From Visual Studio

You can also publish from Visual Studio by importing the MonsterASP.NET WebDeploy publish profile. Make sure the environment variables above are set in the MonsterASP.NET control panel before opening the site.

### HTTPS

Enable Let's Encrypt for the MonsterASP.NET domain in `Domains/HTTPS`. On the free plan, renew the certificate manually every 90 days.

The metadata lookup uses Steam without a key and RAWG when `GameMetadata__RawgApiKey` is configured. If the RAWG key is missing, lookup falls back to Steam-only results.

For local RAWG lookup, keep the key out of source control:

```powershell
dotnet user-secrets init
dotnet user-secrets set "GameMetadata:RawgApiKey" "<RAWG API key>"
```
