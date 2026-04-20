using Microsoft.EntityFrameworkCore;
using TechMoveSystems.Data;
using TechMoveSystems.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var mvcBuilder = builder.Services.AddControllersWithViews();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<TechMoveDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TechMoveDb"),
        sqlOptions => sqlOptions.CommandTimeout(5)));

builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CurrencyApi:BaseUrl"] ?? "https://open.er-api.com/v6/latest/");
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddScoped<ICurrencyCalculator, CurrencyCalculator>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TechMoveDbContext>();
    db.Database.EnsureCreated();
    await SeedData.InitializeAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
