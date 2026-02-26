using System.Diagnostics;
using MenuShopper.Components;
using MenuShopper.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

var staticWebAssetsManifest = Path.Combine(
    builder.Environment.ContentRootPath,
    $"{builder.Environment.ApplicationName}.staticwebassets.runtime.json"
);
if (File.Exists(staticWebAssetsManifest))
    builder.WebHost.UseStaticWebAssets();

var urls = builder.Configuration["Urls"];
if (!string.IsNullOrWhiteSpace(urls))
    builder.WebHost.UseUrls(urls);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<WindowsAutoStartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

var assembly = typeof(Program).Assembly;
var version = assembly.GetName().Version?.ToString() ?? "unknown";
var assemblyLocation = assembly.Location;
if (!string.IsNullOrWhiteSpace(assemblyLocation) && File.Exists(assemblyLocation))
{
    var fileVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
    if (!string.IsNullOrWhiteSpace(fileVersion))
        version = fileVersion;
}

app.Logger.LogInformation(
    "Starting {AppName} v{Version} ({Environment})",
    app.Environment.ApplicationName,
    version,
    app.Environment.EnvironmentName
);

var dataService = app.Services.GetRequiredService<DataService>();
app.Logger.LogInformation("Data folder: {DataFolder}", dataService.GetDataFolderPath());

var meals = await dataService.LoadMealsAsync();
var categories = await dataService.LoadCategoriesAsync();

var menuCount = 0;
try
{
    menuCount = (await dataService.LoadMenusAsync()).Count;
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Couldn't load menus at startup.");
}

var favouriteMeals = meals.Count(m => m.IsFavourite);
var dairyMeals = meals.Count(m => m.IsDairy);
var uniqueIngredients = meals.SelectMany(m => m.Ingredients)
    .Where(i => !string.IsNullOrWhiteSpace(i))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .Count();

app.Logger.LogInformation(
    "Loaded data: {MealCount} meals ({FavouriteCount} favourites, {DairyCount} dairy), {CategoryCount} categories, {MenuCount} menus, {UniqueIngredientCount} unique ingredients.",
    meals.Count,
    favouriteMeals,
    dairyMeals,
    categories.Count,
    menuCount,
    uniqueIngredients
);

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await app.RunAsync();
