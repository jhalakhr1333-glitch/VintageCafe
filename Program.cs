using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using VintageCafe.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ MVC setup
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ✅ Database Connection (your LocalDB)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\rajdejhalak\\Documents\\vintagecafe.mdf;Integrated Security=True;Connect Timeout=30;"
    )
);

// ✅ Authentication (Cookie-based for Users + Admins)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";  // Redirect if not logged in
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "VintageCafe.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// ✅ Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".VintageCafe.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

var app = builder.Build();

// ✅ Exception Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Cookie Policy for session cookies
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.None
});

// ✅ Correct middleware order
app.UseSession();
app.UseAuthentication(); // Must come before Authorization
app.UseAuthorization();

// ✅ Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cafe}/{action=Menu}/{id?}"
);

// ✅ Auto-create Demo Admin & User (if not exists)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Create default demo user
    if (!db.Users.Any(u => u.Email == "jhalak@gmail.com"))
    {
        db.Users.Add(new VintageCafe.Models.User
        {
            Username = "jhalak",
            Email = "jhalak@gmail.com",
            Password = "1234"
        });
    }

    // Create default admin (for Admin Dashboard)
    if (!db.Users.Any(u => u.Email == "JhalakR@vintagecafe.com"))
    {
        db.Users.Add(new VintageCafe.Models.User
        {
            Username = "Admin",
            Email = "JhalakR@vintagecafe.com",
            Password = "jhalak25"
        });
    }

    db.SaveChanges();
}

app.Run();
