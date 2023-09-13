using Microsoft.EntityFrameworkCore;
using MvcIngredient.Data;
using MvcIngredient.Interfaces;
using MvcIngredient.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MvcIngredientContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MvcIngredientContext") ?? throw new InvalidOperationException("Connection string 'MvcIngredientContext' not found.")))
    .AddScoped<IIngredientRepository, IngredientRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Recipe/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Recipe}/{action=Index}");

app.Run();
