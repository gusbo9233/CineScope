# Limiting Actions to Members or Admins

CineScope already has role-based authorization configured in `Program.cs`.

```csharp
options.AddPolicy("Admin", policy =>
    policy.RequireRole("Admin"));

options.AddPolicy("Member", policy =>
    policy.RequireRole("Member", "Admin"));

options.AddPolicy("Guest", policy =>
    policy.RequireAuthenticatedUser());
```

This means:

- `Admin` allows only users with the `Admin` app role.
- `Member` allows users with either `Member` or `Admin`.
- `Guest` allows any signed-in user.

Roles come from the CineScope Entra app registration. Users must be assigned roles in Azure Portal under Enterprise applications -> CineScope -> Users and groups.

## What Users See When Access Is Denied

CineScope is configured to send signed-in users to `/Account/AccessDenied` when they are authenticated but do not have the required role.

This is configured in `Program.cs`:

```csharp
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});
```

The page lives at:

```text
Pages/Account/AccessDenied.cshtml
Pages/Account/AccessDenied.cshtml.cs
```

Use authorization policies on the server-side action or Razor Page. If a signed-in user is missing the required role, ASP.NET Core should redirect them to the friendly Access Denied page instead of showing a browser error.

If you see `HTTP ERROR 405`, it usually means the browser was sent to a route with the wrong HTTP method, such as opening a `POST`-only action with `GET`. Make sure the protected action has the right route and method attributes, and that the user-facing link or form matches them.

## Limit a Controller Action

Add `Authorize` to the controller file:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Then protect individual actions:

```csharp
[Authorize(Policy = "Member")]
public async Task<IActionResult> Create()
{
    return View();
}
```

Only members and admins can use that action.

For admin-only actions:

```csharp
[Authorize(Policy = "Admin")]
public async Task<IActionResult> Create()
{
    // Admin-only work here
}
```

## Limit a Whole Controller

Put the attribute on the controller class:

```csharp
[Authorize(Policy = "Member")]
public class MoviesController : Controller
{
    // Every action here requires Member or Admin
}
```

You can still allow public access to a specific action:

```csharp
[AllowAnonymous]
public IActionResult Search()
{
    return View();
}
```

## Limit a Razor Page

Add `Authorize` to the page model:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Then add the policy attribute:

```csharp
[Authorize(Policy = "Admin")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}
```

The admin dashboard already uses this pattern in `Pages/Admin/Dashboard.cshtml.cs`.

## Hide Buttons and Links in Views

Authorization attributes protect the server-side action. You can also hide UI elements so users do not see actions they cannot use.

Show something only to admins:

```cshtml
@if (User.IsInRole("Admin"))
{
    <a asp-action="Delete" asp-route-id="@movie.Id">Delete</a>
}
```

Show something to members or admins:

```cshtml
@if (User.IsInRole("Member") || User.IsInRole("Admin"))
{
    <a asp-action="Edit" asp-route-id="@movie.Id">Edit</a>
}
```

Important: hiding a button is not security by itself. Always protect the controller action or Razor Page too.

## Recommended Pattern

Use `Member` for normal signed-in app features, such as creating or editing movie entries.

Use `Admin` for destructive or sensitive actions, such as deleting records, managing users, viewing admin dashboards, or changing application-wide settings.

Use `Guest` only when any signed-in user should be allowed, regardless of assigned role.

## Example: Restrict Movie Delete to Admins

In `MoviesController.cs`:

```csharp
[Authorize(Policy = "Admin")]
public async Task<IActionResult> Delete(int id)
{
    // Delete confirmation
}

[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
[Authorize(Policy = "Admin")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    // Actual delete
}
```

In the movie list view:

```cshtml
@if (User.IsInRole("Admin"))
{
    <a asp-action="Delete" asp-route-id="@item.Id">Delete</a>
}
```

## Current Example: Import Movie Is Admin-Only

`MoviesController.ImportMovie` is admin-only, but it handles signed-out users manually instead of relying only on `[Authorize]`.

That is intentional. `ImportMovie` is a `POST`-only action. If `[Authorize]` challenges a signed-out user from a `POST`, the login flow can return to `/Movies/ImportMovie` with `GET`, and that can produce `HTTP ERROR 405`.

Use this pattern for `POST`-only actions that might be submitted by signed-out users:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ImportMovie(string imdbId)
{
    if (User?.Identity?.IsAuthenticated != true)
    {
        return Redirect("/MicrosoftIdentity/Account/SignIn?returnUrl=%2FMovies%2FSearch");
    }

    if (!User.IsInRole("Admin"))
    {
        return RedirectToPage("/Account/AccessDenied");
    }

    // Admin-only work here.
}
```

In the view, show the `POST` form only to admins. Show links for everyone else:

```cshtml
@if (User.IsInRole("Admin"))
{
    <form asp-controller="Movies" asp-action="ImportMovie" method="post">
        @Html.AntiForgeryToken()
        <input type="hidden" name="imdbId" value="@movie.ImdbId" />
        <button type="submit" class="btn btn-primary">Import</button>
    </form>
}
else if (User?.Identity?.IsAuthenticated == true)
{
    <a asp-page="/Account/AccessDenied">Import requires admin</a>
}
else
{
    <a href="/MicrosoftIdentity/Account/SignIn?returnUrl=%2FMovies%2FSearch">Sign in to import</a>
}
```

The server-side checks are still required. Hiding the form only improves the user experience.

## Testing

After changing authorization:

1. Run the app locally.
2. Sign in as a user with the `Admin` role and confirm admin-only actions work.
3. Sign in as a user with the `Member` role and confirm member actions work, but admin actions are blocked.
4. Sign in as a user without the required role and confirm protected actions show `/Account/AccessDenied`.
5. Test a signed-out browser session and confirm protected actions redirect to sign-in.
