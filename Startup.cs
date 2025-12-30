using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;

[assembly: OwinStartup(typeof(Laptop88_3.Startup))]
namespace Laptop88_3
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cookie lưu thông tin đăng nhập chính
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                LogoutPath = new PathString("/Account/Logout"),
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                SlidingExpiration = true
            });

            // Cookie tạm để giữ thông tin khi login ngoài (Google, Facebook, ...)
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Google Login
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "612724816084-kmdit9uqmgllvs86vc8ejg2rbimpeumg.apps.googleusercontent.com",
                ClientSecret = "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",
                CallbackPath = new PathString("/signin-google") // rất quan trọng
            });
        }
    }
}
