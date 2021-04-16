using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SFServerManager.Code.Instanced.Services;


namespace SFServerManager.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private DatabaseService db;

        public LoginModel(DatabaseService dbService)
        {
            db = dbService;
        }
        
        public string ReturnUrl { get; set; }
        public async Task<IActionResult> 
            OnGetAsync(string paramUsername, string paramPassword)
        {
            string returnUrl = Url.Content("~/");
            try
            {
                // Clear the existing external cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }

            var settings = db.FindAll<Setting>("Settings");
            var username = settings.First(x => x.Key == "System.AdminUsername");
            var password = settings.First(x => x.Key == "System.AdminPass");

            if (username.Value != paramUsername || password.Value != paramPassword)
            {
                //Fail
                return LocalRedirect(returnUrl);
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, paramUsername),
                new Claim(ClaimTypes.Role, "Administrator"),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = this.Request.Host.Value
            };
            try
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                              authProperties);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return LocalRedirect(returnUrl);
        }
    }
}
