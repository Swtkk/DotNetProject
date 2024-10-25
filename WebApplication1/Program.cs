using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{ 
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();