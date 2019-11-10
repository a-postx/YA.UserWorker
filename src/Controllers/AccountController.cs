using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace YA.TenantWorker.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrWhiteSpace(userName) &&
                userName == password)
            {
                var claims = new List<Claim>
                {
                    new Claim("sub", "123456789"),
                    new Claim("name", "Dominick"),
                    new Claim("role", "Geek"),
                    new Claim("tenantid", "guid")
                };

                var ci = new ClaimsIdentity(claims, "password", "name", "role");
                var p = new ClaimsPrincipal(ci);

                await HttpContext.SignInAsync(p);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return Redirect("/");
                }
            }

            return View();
        }

        public IActionResult Google(string returnUrl)
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                return Redirect("/");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),

                Items =
                {
                    { "uru", returnUrl },
                    { "scheme", "Google" }
                }
            };

            return Challenge(props, "Google");
        }

        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync("external");
            if (!result.Succeeded)
            {
                throw new Exception("error");
            }

            HttpClient httpClient = new HttpClient();

            //LoginDetails login = new LoginDetails { Username = "admin", Password = "123" };

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44305/api/login");
            request.Content = new StringContent("{\"username\":\"admin\",\"password\":\"123\"}",
                                    Encoding.UTF8,
                                    "application/json");

            var ggg = await httpClient.SendAsync(request);


            ////var ggg = await httpClient.PostAsync("https://localhost:44305/api/login", new StringContent(login.ToString()));

            // get sub and issuer to check if external user is known
            var sub = result.Principal.FindFirst("sub");
            var issuer = result.Properties.Items["scheme"];

            var mmm = await ggg.Content.ReadAsStringAsync();

            // do your customm provisioning logic
            List<Claim> gClaims = result.Principal.Claims.ToList();
            // sign in user
            var claims = new List<Claim>
            {
                new Claim("sub", gClaims.Find(c => c.Type == "sub").Value),
                new Claim("name", result.Principal.Identity.Name),
                new Claim("role", "Geek"),
                new Claim("email", result.Principal.FindFirst("email").Value),
                new Claim("tenantid", result.Principal.Identity.Name)
            };

            var ci = new ClaimsIdentity(claims, issuer, "name", "role");
            var p = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(p);

            return Redirect(result.Properties.Items["uru"]);
        }

        public IActionResult AccessDenied() => View();

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}