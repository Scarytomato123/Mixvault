using MixVault_FrontEnd.Components;
using MixVault_FrontEnd.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MixVault_FrontEnd.Services.Userstate>();
builder.Services.AddSingleton<AudioState>();

builder.Services.AddHttpClient("MixVaultAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7240/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 200 * 1024 * 1024;
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// 1. Zorg dat Antiforgery voor alle interactieve pagina's correct werkt
app.UseAntiforgery();

// 2. Configureer statische bestanden met de juiste audio-mapping
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".mp3"] = "audio/mpeg";
provider.Mappings[".wav"] = "audio/wav";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// 3. Dit is de cruciale stap: de route naar je interactieve components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();