using Microsoft.EntityFrameworkCore;
using SadiclarasanWeb.Data;
using SadiclarasanWeb.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Cookie Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

// HTTPS redirection'ı sadece production'da kullan
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Admin route
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

// İlk admin kullanıcısını otomatik oluştur
 _ = Task.Run(async () => await CreateDefaultAdmin(app));

app.Run();

// Admin kullanıcı oluşturma fonksiyonu
static async Task CreateDefaultAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Veritabanının oluşturulduğundan emin ol
    await context.Database.EnsureCreatedAsync();

    // Varsayılan admini sağla (username veya email ile kontrol)
    var existing = await context.AdminUsers
        .FirstOrDefaultAsync(u => u.Username == "sadiclarsan" || u.Email == "admin@sadiclarsan.com.tr");

    if (existing == null)
    {
        var admin = new AdminUser
        {
            Username = "sadiclarsan",
            Email = "admin@sadiclarsan.com.tr",
            PasswordHash = HashPassword("sadiclarsan2025"),
            FullName = "Sadıçlarsan Admin",
            Role = "Admin",
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        context.AdminUsers.Add(admin);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Default admin user created:");
        Console.WriteLine("   Username: sadiclarsan");
        Console.WriteLine("   Password: sadiclarsan2025");
    }
    else
    {
        existing.Username = "sadiclarsan";
        existing.Email = "admin@sadiclarsan.com.tr";
        existing.PasswordHash = HashPassword("sadiclarsan2025");
        existing.FullName = "Sadıçlarsan Admin";
        existing.Role = "Admin";
        existing.IsActive = true;
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Default admin user ensured (updated):");
        Console.WriteLine("   Username: sadiclarsan");
        Console.WriteLine("   Password: sadiclarsan2025");
    }
}

// Şifre hash fonksiyonu
static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SadiclarasanSalt"));
    return Convert.ToBase64String(hashedBytes);
}
