using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Web.Dev;

internal static class DevAutoLoginMiddleware
{
    private const string SkipCookie = "dev-no-autologin";

    public static IApplicationBuilder UseDevAutoLogin(this IApplicationBuilder app)
        => app.Use(async (ctx, next) =>
        {
            if (!ShouldConsider(ctx))
            {
                await next();
                return;
            }

            if (ctx.Request.Query.ContainsKey("nologin"))
            {
                ctx.Response.Cookies.Append(SkipCookie, "1", new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromHours(8)
                });
                await next();
                return;
            }

            if (ctx.Request.Cookies.ContainsKey(SkipCookie))
            {
                await next();
                return;
            }

            var auth = await ctx.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (auth.Succeeded)
            {
                await next();
                return;
            }

            var devOptions = ctx.RequestServices.GetRequiredService<IOptions<DevOptions>>().Value;
            var userManager = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(devOptions.DefaultUserEmail);
            if (user is null)
            {
                await next();
                return;
            }

            var signInManager = ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            await signInManager.SignInAsync(user, isPersistent: false);
            ctx.Response.Redirect(ctx.Request.GetEncodedUrl());
        });

    private static bool ShouldConsider(HttpContext ctx)
    {
        if (!HttpMethods.IsGet(ctx.Request.Method))
        {
            return false;
        }

        var path = ctx.Request.Path.Value ?? "";
        if (path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/dev", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/_", StringComparison.Ordinal)) return false;
        if (path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}
