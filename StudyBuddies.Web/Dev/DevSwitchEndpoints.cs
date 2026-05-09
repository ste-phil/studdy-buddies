using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Web.Dev;

internal static class DevSwitchEndpoints
{
    public static IEndpointConventionBuilder MapDevSwitchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dev");

        group.MapGet("/switch", async (
            [FromQuery] string userId,
            [FromQuery] string? returnUrl,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, isPersistent: false);

            var target = string.IsNullOrWhiteSpace(returnUrl) ? "~/" : $"~/{returnUrl.TrimStart('/')}";
            return Results.LocalRedirect(target);
        });

        group.MapGet("/logout", async (
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.LocalRedirect("~/?nologin=1");
        });

        return group;
    }
}
