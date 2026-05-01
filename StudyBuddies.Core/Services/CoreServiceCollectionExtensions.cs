using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddStudyBuddiesCore(this IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped<IPartnershipService, PartnershipService>();
        services.AddScoped<IWordService, WordService>();
        services.AddScoped<IStudyService, StudyService>();

        return services;
    }
}
