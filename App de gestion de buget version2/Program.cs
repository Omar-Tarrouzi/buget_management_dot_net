using App_de_gestion_de_buget_version2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configuration de la base de données
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BudgetDB"), sql => sql.EnableRetryOnFailure()));

// NOUVELLE CONFIGURATION IDENTITY
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configuration mot de passe
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Configuration compte
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
// Enable the default Razor UI for Identity (provides /Identity/Account/Login, Register, etc.)
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Ajouter Razor Pages (nécessaire pour Identity)
builder.Services.AddRazorPages();

// Configuration de l'authentification des cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Apply migrations / ensure database and create a default admin user if none exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            // If there are any pending migrations apply them, otherwise ensure database is created from model
            var pending = context.Database.GetPendingMigrations();
            if (pending != null && pending.Any())
            {
                logger.LogInformation("Applying {Count} pending EF Core migrations.", pending.Count());
                context.Database.Migrate();
            }
            else
            {
                logger.LogInformation("No pending migrations found - ensuring database is created.");
                context.Database.EnsureCreated();
            }
        }
        catch (Exception migrateEx)
        {
            logger.LogWarning(migrateEx, "Migrations failed or are not available. Falling back to EnsureCreated().");
            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ensureEx)
            {
                logger.LogError(ensureEx, "EnsureCreated also failed.");
                throw;
            }
        }

        // Create a default admin user for development if no users exist
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        if (!userManager.Users.Any())
        {
            logger.LogInformation("No users found - creating default admin user 'admin' with password 'Admin#123' for development.");
            var admin = new IdentityUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
            var result = userManager.CreateAsync(admin, "Admin#123").GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create default admin user: {Errors}", string.Join(',', result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var log = loggerFactory.CreateLogger("Startup");
        log.LogError(ex, "An error occurred while migrating or initializing the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE CRITIQUE
app.UseAuthentication();
app.UseAuthorization();

// Configuration des routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();