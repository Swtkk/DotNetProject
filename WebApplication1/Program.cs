using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebApplication1.Hubs;
using WebApplication1.Models;
using WebApplication1.Utility;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5001");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{ 
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// options => options.SignIn.RequireConfirmedAccount = true  potwierdzenie maila
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSignalR();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath =$"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});
var app = builder.Build();


if(args.Length ==1 && args[0].ToLower() =="seeddata")
{
    Seed.SeedData(app);
}
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        var canConnect = context.Database.CanConnect();
        Console.WriteLine($"Połączenie z bazą danych: {(canConnect ? "Udane" : "Nieudane")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas próby połączenia: {ex.Message}");
    }
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();