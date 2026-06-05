# CineScope

CineScope is a movie discovery and management web app built with ASP.NET Core MVC. Users can browse a local movie library, search OMDb, import films, and interact with movies through ratings, comments, and favorites.

## Features

- Browse a poster-based movie catalog with ratings and collection stats.
- View movie details, descriptions, IMDb IDs, genres, runtime, and comments.
- Search OMDb by title and import selected movies into the local database.
- Rate movies, post comments, and save personal favorites when signed in.
- Manage movies through admin-only edit, delete, import, seed, and dashboard views.
- Use Microsoft Identity Web for authentication and role-based authorization.

## How to Run

1. Restore dependencies:

   ```bash
   dotnet restore CineScope.slnx
   ```

2. Apply the Entity Framework migrations to the local SQL Server database:

   ```bash
   dotnet ef database update --project CineScope/CineScope.csproj
   ```

3. Run the app:

   ```bash
   dotnet run --project CineScope/CineScope.csproj --launch-profile http
   ```

4. Open the site:

   ```text
   http://localhost:5074
   ```

Local development uses the SQL Server LocalDB connection string in `CineScope/appsettings.Development.json`. The app redirects from `/` to `/Movies`.

## Tech Stack

- ASP.NET Core MVC
- .NET 10
- Razor Views
- Bootstrap
- Entity Framework Core
- SQL Server
- Microsoft Identity Web
- OMDb API
- Azure App Service
- Bicep
- GitHub Actions
