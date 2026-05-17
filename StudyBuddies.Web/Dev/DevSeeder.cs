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

    // Tag-Konvention: Kategorien-Tags auf Deutsch (Tiere, Wohnen, ...) - einheitlich
    // unabhaengig von der Sprachrichtung der Partnerschaft.

    // Authored by Melissa (es) for Philipp (learning es) -- partnership Melissa <-> Philipp
    private static readonly SeedWord[] WordsForPhilipp =
    {
        new("casa", "Haus", "Wohnen"),
        new("agua", "Wasser", "Essen"),
        new("amigo", "Freund", "Familie"),
        new("coche", "Auto", "Verkehr"),
        new("playa", "Strand", "Orte", "Natur"),
        new("mesa", "Tisch", "Wohnen"),
        new("familia", "Familie", "Familie"),
        new("madre", "Mutter", "Familie"),
        new("padre", "Vater", "Familie"),
        new("hermano", "Bruder", "Familie"),
        new("hermana", "Schwester", "Familie"),
        new("mano", "Hand", "Körper"),
        new("ojo", "Auge", "Körper"),
        new("corazón", "Herz", "Körper"),
        new("cabeza", "Kopf", "Körper"),
        new("pie", "Fuß", "Körper"),
        new("hora", "Stunde", "Zeit"),
        new("semana", "Woche", "Zeit"),
        new("mes", "Monat", "Zeit"),
        new("año", "Jahr", "Zeit"),
        new("mañana", "Morgen", "Zeit"),
        new("tarde", "Nachmittag", "Zeit"),
        new("lluvia", "Regen", "Wetter"),
        new("nieve", "Schnee", "Wetter"),
        new("viento", "Wind", "Wetter"),
        new("frío", "kalt", "Wetter", "Adjektive"),
        new("calor", "Hitze", "Wetter"),
        new("correr", "laufen", "Verben"),
        new("comer", "essen", "Verben"),
        new("beber", "trinken", "Verben"),
        // Neue Woerter zur Auffuellung der Kategorien:
        new("ventana", "Fenster", "Wohnen"),
        new("puerta", "Tür", "Wohnen"),
        new("cama", "Bett", "Wohnen"),
        new("cocina", "Küche", "Wohnen"),
        new("abuelo", "Großvater", "Familie"),
        new("abuela", "Großmutter", "Familie"),
        new("brazo", "Arm", "Körper"),
        new("pierna", "Bein", "Körper"),
        new("boca", "Mund", "Körper"),
        new("ayer", "gestern", "Zeit"),
        new("hoy", "heute", "Zeit"),
        new("niebla", "Nebel", "Wetter"),
        new("dormir", "schlafen", "Verben"),
        new("hablar", "sprechen", "Verben"),
    };

    // Authored by Philipp (de) for Melissa (learning de) -- partnership Melissa <-> Philipp
    private static readonly SeedWord[] WordsForMelissaFromPhilipp =
    {
        new("Hund", "perro", "Tiere"),
        new("Apfel", "manzana", "Essen"),
        new("Buch", "libro", "Schule"),
        new("Stuhl", "silla", "Wohnen"),
        new("Tag", "día", "Zeit"),
        new("Nacht", "noche", "Zeit"),
        new("Vogel", "pájaro", "Tiere"),
        new("Pferd", "caballo", "Tiere"),
        new("Fisch", "pez", "Tiere"),
        new("Maus", "ratón", "Tiere"),
        new("Milch", "leche", "Essen"),
        new("Käse", "queso", "Essen"),
        new("Fleisch", "carne", "Essen"),
        new("Reis", "arroz", "Essen"),
        new("Zug", "tren", "Verkehr"),
        new("Fahrrad", "bicicleta", "Verkehr"),
        new("Schule", "escuela", "Schule", "Orte"),
        new("Lehrer", "profesor", "Schule"),
        new("Schüler", "alumno", "Schule"),
        new("Liebe", "amor", "Konzepte"),
        new("Glück", "suerte", "Konzepte"),
        new("Zeit", "tiempo", "Konzepte", "Zeit"),
        new("Geld", "dinero", "Konzepte"),
        new("Arbeit", "trabajo", "Konzepte"),
        new("Garten", "jardín", "Wohnen", "Natur"),
        new("Blume", "flor", "Natur"),
        new("Baum", "árbol", "Natur"),
        new("Berg", "montaña", "Natur", "Orte"),
        new("Fluss", "río", "Natur"),
        new("Meer", "mar", "Natur", "Orte"),
        // Neue Woerter:
        new("Schlange", "serpiente", "Tiere"),
        new("Wolf", "lobo", "Tiere"),
        new("Bär", "oso", "Tiere"),
        new("Kuh", "vaca", "Tiere"),
        new("Ei", "huevo", "Essen"),
        new("Suppe", "sopa", "Essen"),
        new("Flugzeug", "avión", "Verkehr"),
        new("Bus", "autobús", "Verkehr"),
    };

    // Authored by Hannah (de) for Melissa (learning de) -- partnership Melissa <-> Hannah
    private static readonly SeedWord[] WordsForMelissaFromHannah =
    {
        new("Katze", "gato", "Tiere"),
        new("Brot", "pan", "Essen"),
        new("Sonne", "sol", "Wetter", "Natur"),
        new("Kind", "niño", "Familie"),
        new("Frau", "mujer", "Familie"),
        new("Mann", "hombre", "Familie"),
        new("Junge", "chico", "Familie"),
        new("Mädchen", "chica", "Familie"),
        new("Stadt", "ciudad", "Orte"),
        new("Dorf", "pueblo", "Orte"),
        new("Land", "país", "Orte"),
        new("Welt", "mundo", "Orte"),
        new("Himmel", "cielo", "Natur"),
        new("Erde", "tierra", "Natur"),
        new("Stern", "estrella", "Natur"),
        new("Mond", "luna", "Natur"),
        new("Wolke", "nube", "Wetter"),
        new("Sturm", "tormenta", "Wetter"),
        new("Eis", "hielo", "Wetter", "Konzepte"),
        new("Feuer", "fuego", "Natur", "Konzepte"),
        new("Licht", "luz", "Konzepte"),
        new("Farbe", "color", "Farben", "Konzepte"),
        new("weiß", "blanco", "Farben", "Adjektive"),
        new("grau", "gris", "Farben", "Adjektive"),
        new("rosa", "rosa", "Farben", "Adjektive"),
        new("groß", "grande", "Adjektive"),
        new("klein", "pequeño", "Adjektive"),
        new("neu", "nuevo", "Adjektive"),
        new("alt", "viejo", "Adjektive"),
        new("schön", "bonito", "Adjektive"),
        // Neue Woerter:
        new("Sohn", "hijo", "Familie"),
        new("Tochter", "hija", "Familie"),
        new("Wald", "bosque", "Natur"),
        new("See", "lago", "Natur"),
        new("Insel", "isla", "Natur", "Orte"),
        new("Stift", "lápiz", "Schule"),
        new("Donner", "trueno", "Wetter"),
    };

    // Authored by Melissa (es) for Hannah (learning es) -- partnership Melissa <-> Hannah
    private static readonly SeedWord[] WordsForHannah =
    {
        new("verde", "grün", "Farben"),
        new("rojo", "rot", "Farben"),
        new("azul", "blau", "Farben"),
        new("amarillo", "gelb", "Farben"),
        new("negro", "schwarz", "Farben"),
        new("naranja", "orange", "Farben"),
        new("marrón", "braun", "Farben"),
        new("violeta", "lila", "Farben"),
        new("claro", "hell", "Farben", "Adjektive"),
        new("oscuro", "dunkel", "Farben", "Adjektive"),
        new("alto", "hoch", "Adjektive"),
        new("bajo", "niedrig", "Adjektive"),
        new("largo", "lang", "Adjektive"),
        new("corto", "kurz", "Adjektive"),
        new("ancho", "breit", "Adjektive"),
        new("estrecho", "schmal", "Adjektive"),
        new("rápido", "schnell", "Adjektive"),
        new("lento", "langsam", "Adjektive"),
        new("fácil", "einfach", "Adjektive"),
        new("difícil", "schwierig", "Adjektive"),
        new("fuerte", "stark", "Adjektive"),
        new("débil", "schwach", "Adjektive"),
        new("caliente", "heiß", "Adjektive", "Wetter"),
        new("dulce", "süß", "Adjektive", "Essen"),
        new("salado", "salzig", "Adjektive", "Essen"),
        new("amargo", "bitter", "Adjektive", "Essen"),
        new("lleno", "voll", "Adjektive"),
        new("vacío", "leer", "Adjektive"),
        new("limpio", "sauber", "Adjektive"),
        new("sucio", "schmutzig", "Adjektive"),
        // Neue Woerter:
        new("dorado", "golden", "Farben", "Adjektive"),
        new("plateado", "silbern", "Farben", "Adjektive"),
        new("escribir", "schreiben", "Verben"),
        new("leer", "lesen", "Verben"),
        new("escuchar", "hören", "Verben"),
        new("fruta", "Obst", "Essen"),
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
        SeedWord[] words,
        ILogger logger)
    {
        var existingWords = await db.Words
            .Where(w => w.PartnershipId == partnership.Id && w.ByUserId == byUser.Id)
            .ToListAsync();

        var existingByTerm = existingWords.ToDictionary(w => w.Term, StringComparer.Ordinal);
        var added = 0;
        var tagsUpdated = 0;

        foreach (var seed in words)
        {
            var tagList = seed.Tags.ToList();

            if (existingByTerm.TryGetValue(seed.Term, out var existing))
            {
                var oldSet = new HashSet<string>(existing.Tags, StringComparer.OrdinalIgnoreCase);
                var newSet = new HashSet<string>(tagList, StringComparer.OrdinalIgnoreCase);
                if (!oldSet.SetEquals(newSet))
                {
                    existing.Tags = tagList;
                    tagsUpdated++;
                }
                continue;
            }

            db.Words.Add(new Word
            {
                PartnershipId = partnership.Id,
                ByUserId = byUser.Id,
                ForUserId = forUser.Id,
                Term = seed.Term,
                TermLanguage = byUser.NativeLanguage,
                Translation = seed.Translation,
                TranslationLanguage = forUser.NativeLanguage,
                Tags = tagList,
                CreatedAt = DateTime.UtcNow
            });
            added++;
        }

        if (added > 0 || tagsUpdated > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation(
                "DevSeeder added {Added} word(s) and updated tags on {Updated} word(s) by {ByEmail} for {ForEmail}",
                added, tagsUpdated, byUser.Email, forUser.Email);
        }
    }

    private record DevUser(string Email, string DisplayName, string NativeLanguage);

    private sealed record SeedWord(string Term, string Translation, params string[] Tags);
}
