using E_ticaret_Sitesi.Models; // OnlineShoppingContext sýnýfý buradan
using Microsoft.EntityFrameworkCore; // Veritabaný iþlemleri için

var builder = WebApplication.CreateBuilder(args);

// Veritabanýný baðla
builder.Services.AddDbContext<OnlineShoppingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC ve Session'ý ekle
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession(); // Session'ý aktif et

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
