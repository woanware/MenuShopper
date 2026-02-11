using MenuShopper.Components;
using MenuShopper.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

var staticWebAssetsManifest = Path.Combine(
    builder.Environment.ContentRootPath,
    $"{builder.Environment.ApplicationName}.staticwebassets.runtime.json");
if (File.Exists(staticWebAssetsManifest))
    builder.WebHost.UseStaticWebAssets();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DataService>();

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

_ = app.Services.GetRequiredService<DataService>();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
