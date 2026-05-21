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

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7240/")
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Zet de maximale berichtgrootte voor uploads op 200 MB
        options.MaximumReceiveMessageSize = 200 * 1024 * 1024;
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30); // Voorkomt time-out bij traag uploaden
    });

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
// Voeg audio mapping toe zodat de server weet dat dit veilige streams zijn
provider.Mappings[".mp3"] = "audio/mpeg";
provider.Mappings[".wav"] = "audio/wav";

var staticFileOptions = new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "",
    ContentTypeProvider = provider // <--- Koppeling van de audio-rechten
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

app.UseStaticFiles(staticFileOptions);


app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
