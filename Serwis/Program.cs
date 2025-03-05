var builder = WebApplication.CreateBuilder(args);

// Rejestracja HttpClient
builder.Services.AddHttpClient();

// ✅ Konfiguracja CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins("http://localhost:5001") // Zezwalamy na WebApplication1
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Ustawienie konkretnego portu
builder.WebHost.UseUrls("http://localhost:5002");

builder.Services.AddControllers();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// ✅ Dodaj obsługę CORS przed UseAuthorization
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();
app.MapControllers();

app.Run();