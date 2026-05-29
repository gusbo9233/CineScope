using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineScope.Pages.Account
{
    public class SignoutModel : PageModel
    {
        public async Task OnGet()
        {
            await HttpContext.SignOutAsync("OpenIdConnect");
        }
    }
}
