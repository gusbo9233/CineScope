using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineScope.Pages.Account
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string returnUrl = "/")
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return Redirect(returnUrl);
            }

            var props = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
