using E_ticaret_Sitesi.Models; // OnlineShoppingContext s�n�f� buradan
using Microsoft.EntityFrameworkCore; // Veritaban� i�lemleri i�in

var builder = WebApplication.CreateBuilder(args);

// Veritaban�n� ba�la
builder.Services.AddDbContext<OnlineShoppingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC ve Session'� ekle
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession(); // Session'� aktif et

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
