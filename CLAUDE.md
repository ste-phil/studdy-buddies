# StudyBuddies — Project Context

A .NET 10 + Blazor Web App for **paired vocabulary learning**. Two users form a Partnership and configure vocabulary FOR each other. Each word has a term in the author's native language plus a translation in the partner's native language; the partner learns it via SM-2 flashcards.

## Stack

- **.NET 10** (SDK 10.0.102)
- **Blazor Web App** with `InteractiveServer` rendering globally — no WASM
- **SQLite + EF Core 10** (single file `StudyBuddies.Web/app.db`, gitignored)
- **ASP.NET Core Identity** (email + password, RequireConfirmedAccount = false)
- **MudBlazor 9.4.0** for UI
- **flag-icons** (CDN: `https://cdn.jsdelivr.net/gh/lipis/flag-icons@7.2.3/css/flag-icons.min.css`) for language flags
- **IStringLocalizer + .resx** for i18n (`de` + `es`)
- **No Aspire** (single project setup is sufficient)

## Solution layout

```
study-buddies/
├── StudyBuddies.slnx
├── StudyBuddies.Core/                  (class library)
│   ├── Data/
│   │   ├── ApplicationUser.cs          (extends IdentityUser; DisplayName, NativeLanguage)
│   │   ├── ApplicationDbContext.cs     (IdentityDbContext<ApplicationUser> + domain entities)
│   │   ├── Partnership.cs              (+ PartnershipStatus enum)
│   │   ├── Word.cs
│   │   ├── Review.cs                   (1:1 with Word, SM-2 state)
│   │   └── Migrations/
│   └── Services/
│       ├── IPartnershipService + PartnershipService
│       ├── IWordService + WordService
│       ├── IStudyService + StudyService
│       ├── Sm2Service                  (pure SM-2 function)
│       ├── Dtos.cs                     (UserSummary, PartnershipSummary, WordSummary, StudyCard, …)
│       └── CoreServiceCollectionExtensions   (AddStudyBuddiesCore extension)
└── StudyBuddies.Web/
    ├── Components/
    │   ├── App.razor                    (host; <Routes @rendermode="InteractiveServer" />)
    │   ├── Routes.razor
    │   ├── RedirectToLogin.razor
    │   ├── Layout/MainLayout.razor      (MudLayout + AppBar + Drawer + Mud*Provider)
    │   ├── Layout/NavMenu.razor
    │   ├── Pages/                       (Home, Dashboard, Partners, PartnersSearch,
    │   │                                  PartnerDetail, WordNew, WordDetail, Study, …)
    │   ├── Shared/LanguageSwitcher.razor
    │   ├── Shared/WordsTable.razor
    │   └── Account/                      (ASP.NET Identity Razor pages, lightly modified)
    ├── Resources/SharedResource.{de,es}.resx
    ├── SharedResource.cs                 (marker class — MUST be in root namespace, see gotcha #3)
    ├── wwwroot/js/speech.js              (window.studyBuddiesSpeech + studyBuddiesLang)
    ├── Program.cs                         (DI: AddStudyBuddiesCore, AddMudServices,
    │                                       AddLocalization, UseRequestLocalization)
    └── appsettings.json                   (DefaultConnection = "DataSource=app.db;Cache=Shared")
```

## Architecture rules

1. **Blazor components must not touch EF Core directly.** They depend on service interfaces only (`IPartnershipService`, `IWordService`, `IStudyService`). This keeps the door open for a future `StudyBuddies.Api` project that reuses the same services.
2. `ApplicationDbContext` lives in **Core**. The Web project registers it via `services.AddStudyBuddiesCore(connectionString)` — no direct `AddDbContext` in `Program.cs`.
3. All services are scoped and registered in `CoreServiceCollectionExtensions.AddStudyBuddiesCore`.

## Domain rules

- A `Partnership` always stores `UserAId` < `UserBId` ordinal-wise (so a single unique index `(UserAId, UserBId)` prevents duplicates regardless of who initiated). `RequestedById` records who initiated the request.
- Status flow: `Pending` → `Active` (recipient accepts) **or** `Declined` (rejected). Only the recipient (≠ `RequestedById`) may accept.
- A `Word` belongs to **one** Partnership. `ForUserId` = the learner, `ByUserId` = the author. `TermLanguage` = `ByUser.NativeLanguage`, `TranslationLanguage` = `ForUser.NativeLanguage` (auto-filled from profile).
- Only the author (`ByUser`) can edit or delete a Word.
- `Review` is 1:1 with `Word`. When a learner first grades a Word, a `Review` row is created and tracked via SM-2.
- Multiple partnerships per user are allowed.

## Common commands

```bash
# Build
dotnet build StudyBuddies.slnx

# Run (HTTP only, port 5227)
cd StudyBuddies.Web && dotnet run

# Run (HTTPS profile, port 7281)
cd StudyBuddies.Web && dotnet run --launch-profile https

# Hot reload (recommended during dev — Razor edits picked up without restart)
cd StudyBuddies.Web && dotnet watch

# Add new EF migration
dotnet ef migrations add <Name> --project StudyBuddies.Core \
    --startup-project StudyBuddies.Web --output-dir Data/Migrations

# Apply migrations manually (also runs automatically at startup via db.Database.Migrate())
dotnet ef database update --project StudyBuddies.Core --startup-project StudyBuddies.Web

# Reset DB
rm StudyBuddies.Web/app.db
```

## Key decisions / gotchas

1. **No `StudyBuddies.Web.Client` (WASM) project.** The auto-generated Client was removed because every page needs EF Core, which is server-only. Without WASM, `Routes.razor` declares `@rendermode="InteractiveServer"` globally so MudBlazor providers and interactive components share a single render tree.
2. **MudBlazor providers must be in the same render tree as interactive children.** A `MudPopoverProvider` in a static parent and an interactive `MudMenu` in a child throws *"Missing &lt;MudPopoverProvider /&gt;"* at runtime. Fix: keep the global `@rendermode="InteractiveServer"` on `<Routes />`.
3. **`SharedResource.cs` must live in the root namespace.** With `ResourcesPath = "Resources"` and the marker class at `StudyBuddies.Web.Resources.SharedResource`, the localizer constructs the resource base name as `Resources.Resources.SharedResource.{culture}.resources` — no match. Putting the class at `StudyBuddies.Web.SharedResource` (file at the project root) gives the correct base name `Resources.SharedResource.{culture}.resources`. The `.resx` files stay in `Resources/`.
4. **Default culture is `es` (Spanish).** Supported cultures: `de`, `es`. Configured in `Program.cs` via `app.UseRequestLocalization(...)`.
5. **Language switcher** (`Components/Shared/LanguageSwitcher.razor`) writes the `.AspNetCore.Culture` cookie via JS interop (`window.studyBuddiesLang.set`) and reloads the page. Cookie format: `c=<culture>|uic=<culture>`, URL-encoded.
6. **Email confirmation is disabled** (`RequireConfirmedAccount = false`) because no SMTP/email sender is wired up. `IdentityNoOpEmailSender` is registered as a placeholder.
7. **Auto-apply migrations at startup**: `Program.cs` calls `db.Database.Migrate()` once on startup. Convenient for dev with SQLite; review for production.
8. **Connection string**: `appsettings.json` → `ConnectionStrings:DefaultConnection = "DataSource=app.db;Cache=Shared"`. The DB lives next to the Web project executable.

## Localization keys

Keys are namespaced (e.g. `Auth.Login`, `Partners.IncomingRequests`, `Word.NewSubtext`, `Status.New`). When adding a new UI string, add it to **both** `Resources/SharedResource.de.resx` and `Resources/SharedResource.es.resx`. The XML is hand-written; format-string placeholders use `{0}`, `{1}`.

## Communication style with the user

- **DE:** Antworte standardmäßig zweisprachig (Deutsch + Spanisch). Der Nutzer ist Spanisch-Muttersprachler und lernt Deutsch — beide Sprachen helfen.
- **ES:** Responde por defecto en bilingüe (alemán + español). El usuario es hispanohablante aprendiendo alemán — ambos idiomas ayudan.
- Bei `AskUserQuestion` IMMER beide Sprachen pro Frage/Option. / En `AskUserQuestion` siempre ambos idiomas en pregunta y opciones.
- Code, Pfade, Identifier bleiben Englisch. / Código, paths e identificadores se quedan en inglés.
- Persistent memory: `C:\Users\phste\.claude\projects\D---Entwicklung-study-buddies\memory\` contains feedback notes.

## Working preferences

- **Pragmatic over over-engineered.** User explicitly rejected adding .NET Aspire ("we don't need it for one project"). Don't propose extra layers/projects/abstractions unless a current need justifies them.
- **No comments unless they explain non-obvious "why".** Keep code clean.
- Single git repo at project root. `master` branch. `.gitignore` excludes bin/, obj/, *.db, .playwright-mcp/, .claude/settings.local.json.

## Not yet built

- Profile editing (change `NativeLanguage` after registration)
- Tag autocomplete from existing tags
- Dashboard streaks/charts beyond simple counts
- Email confirmation / password reset (needs SMTP)
- Dockerfile (deferred until hosting decision)
- `StudyBuddies.Api` project (architecture supports it; create only when a non-Blazor frontend is needed)

## Reference plan

The full design plan lives at `C:\Users\phste\.claude\plans\kind-jingling-hopper.md` (approved 2026-05-01).
