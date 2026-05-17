using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Web.Dev;

internal static class DevSeeder
{
    private const string Password = "Pa$$w0rd!";

    private static readonly DevUser Melissa = new("melissa@dev.local", "Melissa", "es");
    private static readonly DevUser Philipp = new("philipp@dev.local", "Philipp", "de");
    private static readonly DevUser Hannah = new("hannah@dev.local", "Hannah", "de");

    // Authored by Melissa (es) for Philipp (learning es) -- partnership Melissa <-> Philipp
    private static readonly (string Term, string Translation)[] WordsForPhilipp =
    {
        ("casa", "Haus"),
        ("agua", "Wasser"),
        ("amigo", "Freund"),
        ("coche", "Auto"),
        ("playa", "Strand"),
        ("mesa", "Tisch"),
        ("familia", "Familie"),
        ("madre", "Mutter"),
        ("padre", "Vater"),
        ("hermano", "Bruder"),
        ("hermana", "Schwester"),
        ("mano", "Hand"),
        ("ojo", "Auge"),
        ("corazón", "Herz"),
        ("cabeza", "Kopf"),
        ("pie", "Fuß"),
        ("hora", "Stunde"),
        ("semana", "Woche"),
        ("mes", "Monat"),
        ("año", "Jahr"),
        ("mañana", "Morgen"),
        ("tarde", "Nachmittag"),
        ("lluvia", "Regen"),
        ("nieve", "Schnee"),
        ("viento", "Wind"),
        ("frío", "kalt"),
        ("calor", "Hitze"),
        ("correr", "laufen"),
        ("comer", "essen"),
        ("beber", "trinken")
    };

    // Authored by Philipp (de) for Melissa (learning de) -- partnership Melissa <-> Philipp
    private static readonly (string Term, string Translation)[] WordsForMelissaFromPhilipp =
    {
        ("Hund", "perro"),
        ("Apfel", "manzana"),
        ("Buch", "libro"),
        ("Stuhl", "silla"),
        ("Tag", "día"),
        ("Nacht", "noche"),
        ("Vogel", "pájaro"),
        ("Pferd", "caballo"),
        ("Fisch", "pez"),
        ("Maus", "ratón"),
        ("Milch", "leche"),
        ("Käse", "queso"),
        ("Fleisch", "carne"),
        ("Reis", "arroz"),
        ("Zug", "tren"),
        ("Fahrrad", "bicicleta"),
        ("Schule", "escuela"),
        ("Lehrer", "profesor"),
        ("Schüler", "alumno"),
        ("Liebe", "amor"),
        ("Glück", "suerte"),
        ("Zeit", "tiempo"),
        ("Geld", "dinero"),
        ("Arbeit", "trabajo"),
        ("Garten", "jardín"),
        ("Blume", "flor"),
        ("Baum", "árbol"),
        ("Berg", "montaña"),
        ("Fluss", "río"),
        ("Meer", "mar")
    };

    // Authored by Hannah (de) for Melissa (learning de) -- partnership Melissa <-> Hannah
    private static readonly (string Term, string Translation)[] WordsForMelissaFromHannah =
    {
        ("Katze", "gato"),
        ("Brot", "pan"),
        ("Sonne", "sol"),
        ("Kind", "niño"),
        ("Frau", "mujer"),
        ("Mann", "hombre"),
        ("Junge", "chico"),
        ("Mädchen", "chica"),
        ("Stadt", "ciudad"),
        ("Dorf", "pueblo"),
        ("Land", "país"),
        ("Welt", "mundo"),
        ("Himmel", "cielo"),
        ("Erde", "tierra"),
        ("Stern", "estrella"),
        ("Mond", "luna"),
        ("Wolke", "nube"),
        ("Sturm", "tormenta"),
        ("Eis", "hielo"),
        ("Feuer", "fuego"),
        ("Licht", "luz"),
        ("Farbe", "color"),
        ("weiß", "blanco"),
        ("grau", "gris"),
        ("rosa", "rosa"),
        ("groß", "grande"),
        ("klein", "pequeño"),
        ("neu", "nuevo"),
        ("alt", "viejo"),
        ("schön", "bonito")
    };

    // Authored by Melissa (es) for Hannah (learning es) -- partnership Melissa <-> Hannah
    private static readonly (string Term, string Translation)[] WordsForHannah =
    {
        ("verde", "grün"),
        ("rojo", "rot"),
        ("azul", "blau"),
        ("amarillo", "gelb"),
        ("negro", "schwarz"),
        ("naranja", "orange"),
        ("marrón", "braun"),
        ("violeta", "lila"),
        ("claro", "hell"),
        ("oscuro", "dunkel"),
        ("alto", "hoch"),
        ("bajo", "niedrig"),
        ("largo", "lang"),
        ("corto", "kurz"),
        ("ancho", "breit"),
        ("estrecho", "schmal"),
        ("rápido", "schnell"),
        ("lento", "langsam"),
        ("fácil", "einfach"),
        ("difícil", "schwierig"),
        ("fuerte", "stark"),
        ("débil", "schwach"),
        ("caliente", "heiß"),
        ("dulce", "süß"),
        ("salado", "salzig"),
        ("amargo", "bitter"),
        ("lleno", "voll"),
        ("vacío", "leer"),
        ("limpio", "sauber"),
        ("sucio", "schmutzig")
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DevSeeder");
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        var melissa = await EnsureUserAsync(userManager, Melissa, logger);
        var philipp = await EnsureUserAsync(userManager, Philipp, logger);
        var hannah = await EnsureUserAsync(userManager, Hannah, logger);

        var philippMelissa = await EnsurePartnershipAsync(db, philipp, melissa, logger);
        await EnsureWordsAsync(db, philippMelissa, byUser: melissa, forUser: philipp, WordsForPhilipp, logger);
        await EnsureWordsAsync(db, philippMelissa, byUser: philipp, forUser: melissa, WordsForMelissaFromPhilipp, logger);

        var melissaHannah = await EnsurePartnershipAsync(db, melissa, hannah, logger);
        await EnsureWordsAsync(db, melissaHannah, byUser: hannah, forUser: melissa, WordsForMelissaFromHannah, logger);
        await EnsureWordsAsync(db, melissaHannah, byUser: melissa, forUser: hannah, WordsForHannah, logger);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        DevUser seed,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(seed.Email);
        if (existing is not null)
        {
            var changed = false;
            if (existing.DisplayName != seed.DisplayName)
            {
                existing.DisplayName = seed.DisplayName;
                changed = true;
            }
            if (existing.NativeLanguage != seed.NativeLanguage)
            {
                existing.NativeLanguage = seed.NativeLanguage;
                changed = true;
            }
            if (changed)
            {
                await userManager.UpdateAsync(existing);
                logger.LogInformation("DevSeeder updated user {Email} (DisplayName/NativeLanguage)", seed.Email);
            }
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = seed.Email,
            Email = seed.Email,
            EmailConfirmed = true,
            DisplayName = seed.DisplayName,
            NativeLanguage = seed.NativeLanguage
        };

        var result = await userManager.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"DevSeeder failed to create {seed.Email}: {errors}");
        }

        logger.LogInformation("DevSeeder created user {Email}", seed.Email);
        return user;
    }

    private static async Task<Partnership> EnsurePartnershipAsync(
        ApplicationDbContext db,
        ApplicationUser requester,
        ApplicationUser other,
        ILogger logger)
    {
        var (userAId, userBId) = string.CompareOrdinal(requester.Id, other.Id) < 0
            ? (requester.Id, other.Id)
            : (other.Id, requester.Id);

        var existing = await db.Partnerships
            .FirstOrDefaultAsync(p => p.UserAId == userAId && p.UserBId == userBId);

        if (existing is not null)
        {
            if (existing.Status != PartnershipStatus.Active)
            {
                existing.Status = PartnershipStatus.Active;
                existing.AcceptedAt ??= DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return existing;
        }

        var partnership = new Partnership
        {
            UserAId = userAId,
            UserBId = userBId,
            RequestedById = requester.Id,
            Status = PartnershipStatus.Active,
            CreatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow
        };

        db.Partnerships.Add(partnership);
        await db.SaveChangesAsync();
        logger.LogInformation("DevSeeder created active partnership {PartnershipId}", partnership.Id);
        return partnership;
    }

    private static async Task EnsureWordsAsync(
        ApplicationDbContext db,
        Partnership partnership,
        ApplicationUser byUser,
        ApplicationUser forUser,
        (string Term, string Translation)[] words,
        ILogger logger)
    {
        var existingTerms = await db.Words
            .Where(w => w.PartnershipId == partnership.Id && w.ByUserId == byUser.Id)
            .Select(w => w.Term)
            .ToListAsync();

        var existing = new HashSet<string>(existingTerms, StringComparer.Ordinal);
        var added = 0;

        foreach (var (term, translation) in words)
        {
            if (existing.Contains(term))
            {
                continue;
            }

            db.Words.Add(new Word
            {
                PartnershipId = partnership.Id,
                ByUserId = byUser.Id,
                ForUserId = forUser.Id,
                Term = term,
                TermLanguage = byUser.NativeLanguage,
                Translation = translation,
                TranslationLanguage = forUser.NativeLanguage,
                CreatedAt = DateTime.UtcNow
            });
            added++;
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation(
                "DevSeeder added {Count} words by {ByEmail} for {ForEmail}",
                added, byUser.Email, forUser.Email);
        }
    }

    private record DevUser(string Email, string DisplayName, string NativeLanguage);
}
