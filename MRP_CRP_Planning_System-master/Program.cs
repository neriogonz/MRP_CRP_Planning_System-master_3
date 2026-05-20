using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

 
    await WaitForDatabase(dbContext);
    
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Database tables created!");


    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("Checking for pending migrations...");
        
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("Database is up to date.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw; 
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();


static async Task WaitForDatabase(AppDbContext context)
{
    for (int i = 0; i < 10; i++)
    {
        if (await context.Database.CanConnectAsync())
        {
            Console.WriteLine("Database connected!");
            return;
        }
        else
        {
            Console.WriteLine($"Waiting for database... ({i + 1}/10)");
            await Task.Delay(3000);
        }
    }
}
