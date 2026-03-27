using MediaLibrary.Components;
using MediaLibrary.Data;
using MediaLibrary.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<MediaRepository>();
builder.Services.AddScoped<ConnectionRepository>();
builder.Services.AddScoped<SettingsRepository>();
builder.Services.AddScoped<ThumbnailService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Initialise database (creates tables + seeds defaults)
app.Services.GetRequiredService<DatabaseService>().Initialize();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Serve media files from the configured storage path under /media
var mediaPath = app.Configuration["Media:StoragePath"] ?? "/data/media";
Directory.CreateDirectory(mediaPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(mediaPath)),
    RequestPath = "/media"
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
