using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mixvault_API.Models;
using Microsoft.AspNetCore.Http;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// --- VOEG DIT TOE: Vertel de webserver (Kestrel) dat grote bestanden (200 MB) zijn toegestaan ---
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 200 * 1024 * 1024;
});

var connectionString = "Server=localhost;Database=MixVault;User=root;Password=1234;";
builder.Services.AddDbContext<MixVault>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// GET: alle users
app.MapGet("/users", async (MixVault db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
});


// GET: Alle afspeellijsten (playlists) ophalen
app.MapGet("/playlists", async (MixVault db) =>
{
    // We halen de afspeellijsten op uit de database
    var playlists = await db.Playlists
        .Select(p => new
        {
            p.PlaylistId,
            p.PlaylistName,
            p.PlaylistDescription,
            p.PlaylistGenre,
            p.PlaylistTags
        })
        .ToListAsync();

    return Results.Ok(playlists);
});

// GET: Alle nummers (tracks) ophalen inclusief de naam van de uploader uit de Users tabel!
app.MapGet("/tracks", async (MixVault db) =>
{
    var tracks = await db.Tracks
        .Include(t => t.FkUserNavigation) // Zorgt ervoor dat Entity Framework de User-data inlaadt
        .Select(t => new
        {
            t.TrackId,
            t.Title,
            t.TrackArtist,
            t.TrackGenre,
            t.DurationMs,
            t.TrackUploadedAt,
            t.TrackFileUrl,
            UploaderName = t.FkUserNavigation != null ? t.FkUserNavigation.DisplayName : "Onbekende DJ" // <-- HIER FIXEN WE DE NAAM!
        })
        .ToListAsync();
    return Results.Ok(tracks);
});
//GET: kijk als wachtwoord en username overeenkomen en link met id
app.MapGet("/login", async (string username, string password, MixVault db) =>
{
    var user = await db.Users
        .Where(u => u.DisplayName == username && u.Password == password)
        .Select(u => new { IdUser = u.UserId, NameUser = u.DisplayName })
        .FirstOrDefaultAsync();

    if (user == null)
        return Results.Unauthorized();

    return Results.Ok(user);
});


// GET: Alle nummers van een specifieke afspeellijst ophalen
app.MapGet("/playlists/{id}/tracks", async (int id, MixVault db) =>
{
    // We zoeken in de koppeltabel 'PlaylistHasTracks' naar de juiste nummers
    var tracksInPlaylist = await db.Playlisthastracks
        .Where(pht => pht.FkPlaylist == id)
        .Include(pht => pht.FkTrackNavigation) // Haal de gegevens van het nummer zelf op
        .OrderBy(pht => pht.Position) // Sorteer ze op de juiste volgorde
        .Select(pht => new
        {
            Position = pht.Position,
            TrackId = pht.FkTrackNavigation.TrackId,
            Title = pht.FkTrackNavigation.Title,
            Duration = pht.FkTrackNavigation.DurationMs
        })
        .ToListAsync();

    return Results.Ok(tracksInPlaylist);
});

// POST: Een nummer liken
app.MapPost("/users/{userId}/likes/tracks/{trackId}", async (int userId, int trackId, MixVault db) =>
{
    // Eerst controleren we of deze gebruiker dit nummer al geliket heeft om dubbele likes te voorkomen
    var exists = await db.Userlikestracks.AnyAsync(ult => ult.FkUser == userId && ult.FkTrack == trackId);
    if (exists) return Results.Conflict("Je hebt dit nummer al geliket.");

    // Maak de nieuwe like aan en sla hem op in de database
    var newLike = new Userlikestrack { FkUser = userId, FkTrack = trackId };
    db.Userlikestracks.Add(newLike);
    await db.SaveChangesAsync();

    return Results.Ok("Nummer geliket!");
});

// DELETE: Een like van een nummer weghalen (un-liken)
app.MapDelete("/users/{userId}/likes/tracks/{trackId}", async (int userId, int trackId, MixVault db) =>
{
    // Zoek de bestaande like op
    var like = await db.Userlikestracks.FirstOrDefaultAsync(ult => ult.FkUser == userId && ult.FkTrack == trackId);
    if (like == null) return Results.NotFound("Like niet gevonden.");

    // Verwijder de like uit de database
    db.Userlikestracks.Remove(like);
    await db.SaveChangesAsync();

    return Results.Ok("Like verwijderd!");
});


// POST: Een playlist liken
app.MapPost("/users/{userId}/likes/playlist/{playlisId}", async (int userId, int trackId, MixVault db) =>
{
    // Eerst controleren we of deze gebruiker dit nummer al geliket heeft om dubbele likes te voorkomen
    var exists = await db.Userlikesplaylists.AnyAsync(ult => ult.FkUser == userId && ult.FkPlaylist == trackId);
    if (exists) return Results.Conflict("Je hebt dit nummer al geliket.");

    // Maak de nieuwe like aan en sla hem op in de database
    var newLike = new Userlikesplaylist { FkUser = userId, FkPlaylist = trackId };
    db.Userlikesplaylists.Add(newLike);
    await db.SaveChangesAsync();

    return Results.Ok("Playlist geliket!");
});

// DELETE: Een like van een playlist weghalen (un-liken)
app.MapDelete("/users/{userId}/likes/playlist/{playlistId}", async (int userId, int trackId, MixVault db) =>
{
    // Zoek de bestaande like op
    var like = await db.Userlikesplaylists.FirstOrDefaultAsync(ult => ult.FkUser == userId && ult.FkPlaylist == trackId);
    if (like == null) return Results.NotFound("Playlist niet gevonden.");

    // Verwijder de like uit de database
    db.Userlikesplaylists.Remove(like);
    await db.SaveChangesAsync();

    return Results.Ok("Playlist verwijderd!");
});
// POST: Nummer uploaden en opslaan in de JUISTE map
app.MapPost("/tracks", async (HttpRequest request, MixVault db) =>
{
    if (!request.HasFormContentType || !request.Form.Files.Any())
    {
        return Results.BadRequest("Geen audiobestand ontvangen.");
    }

    var file = request.Form.Files[0];

    var title = request.Form["title"].ToString();
    var artist = request.Form["artist"].ToString();
    var genre = request.Form["genre"].ToString();
    var fkUser = int.Parse(request.Form["fkUser"].ToString());

    var durationMsStr = request.Form["durationMs"].ToString();
    double? durationMs = double.TryParse(durationMsStr, out var d) ? d : null;

    if (string.IsNullOrEmpty(title)) return Results.BadRequest("Titel is verplicht.");

    // --- BESTAND OPSLAAN IN DE CORRECTE SUBMAP ---
    var fileExtension = Path.GetExtension(file.FileName);
    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}"; // Alleen de GUID + Extensie houdt de URL schoon

    // We slaan hem op in wwwroot/uploads/tracks zodat het stream-endpoint hem vindt!
    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

    var filePath = Path.Combine(uploadFolder, uniqueFileName);
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // --- DATABASE OPSLAAN ---
    var newTrack = new Track
    {
        Title = title,
        TrackArtist = artist,
        TrackGenre = genre,
        FkUser = fkUser,
        DurationMs = durationMs,
        TrackUploadedAt = DateTime.Now,
        // We slaan hier direct de URL op die naar je werkende stream-endpoint verwijst!
        TrackFileUrl = $"/tracks/stream/{uniqueFileName}"
    };

    db.Tracks.Add(newTrack);
    await db.SaveChangesAsync();

    return Results.Created($"/tracks/{newTrack.TrackId}", newTrack);
});
// DELETE: Verwijder een track (Alleen via 'Mijn Uploads')
app.MapDelete("/tracks/{id:int}", async (int id, MixVault db) =>
{
    var track = await db.Tracks.FindAsync(id);
    if (track == null) return Results.NotFound("Track niet gevonden.");

    // Verwijder het fysieke mp3-bestand van de server
    if (!string.IsNullOrEmpty(track.TrackFileUrl))
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", track.TrackFileUrl.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    db.Tracks.Remove(track);
    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "Track succesvol uit je kluis gewist." });
});
// POST: Een nieuwe afspeellijst maken
app.MapPost("/playlists", async (Playlist newPlaylist, MixVault db) =>
{
    newPlaylist.PlaylistCreatedAt = DateTime.Now;

    db.Playlists.Add(newPlaylist);
    await db.SaveChangesAsync();

    return Results.Created($"/playlists/{newPlaylist.PlaylistId}", newPlaylist);
});

// POST: Maak een nieuwe playlist aan voor een specifieke user
app.MapPost("/users/{userId}/playlists", async (int userId, Playlist newPlaylist, MixVault db) =>
{
    // Koppel de playlist aan de maker (let op: check even of jouw property FkUser of UserId heet in je Playlist model!)
    newPlaylist.FkUser = userId;

    db.Playlists.Add(newPlaylist);
    await db.SaveChangesAsync();

    // We sturen het hele object terug (inclusief de nieuw gegenereerde PlaylistId) 
    // zodat Blazor weet in welke ID de track direct daarna gestoken moet worden!
    return Results.Ok(newPlaylist);
});

// GET: Alle gelikete nummers van een specifieke gebruiker ophalen met uploader naam
app.MapGet("/users/{userId}/likes/tracks", async (int userId, MixVault db) =>
{
    var likedTracks = await db.Userlikestracks
        .Where(u => u.FkUser == userId)
        .Include(u => u.FkTrackNavigation)
        .ThenInclude(t => t.FkUserNavigation) // Laad de user van de track in
        .Select(u => new
        {
            u.FkTrackNavigation.TrackId,
            u.FkTrackNavigation.Title,
            TrackArtist = u.FkTrackNavigation.TrackArtist,
            TrackGenre = u.FkTrackNavigation.TrackGenre,
            u.FkTrackNavigation.DurationMs,
            u.FkTrackNavigation.TrackUploadedAt,
            TrackFileUrl = u.FkTrackNavigation.TrackFileUrl,
            UploaderName = u.FkTrackNavigation.FkUserNavigation != null ? u.FkTrackNavigation.FkUserNavigation.DisplayName : "Onbekende DJ" // <-- HIER OOK!
        })
        .ToListAsync();
    return Results.Ok(likedTracks);
});

// GET: Alle gelikte afspeellijsten van een specifieke gebruiker ophalen
app.MapGet("/users/{userId}/likes/playlists", async (int userId, MixVault db) =>
{
    var likedPlaylists = await db.Userlikesplaylists
        .Where(u => u.FkUser == userId) // <--- Veranderd naar 'u' voor consistentie
        .Include(u => u.FkPlaylistNavigation)
        .Select(u => new // <--- Veranderd naar 'u'
        {
            u.FkPlaylistNavigation.PlaylistId,
            u.FkPlaylistNavigation.PlaylistName,
            u.FkPlaylistNavigation.PlaylistDescription,
            u.FkPlaylistNavigation.PlaylistGenre,
            u.FkPlaylistNavigation.PlaylistTags
        })
        .ToListAsync();
    return Results.Ok(likedPlaylists);
});

// GET: Haal alle eigen playlists van een user op
app.MapGet("/users/{userId}/playlists", async (int userId, MixVault db) =>
{
    var myPlaylists = await db.Playlists
        // LET OP: Controleer of dit FkUser heet in jouw C# model, of misschien UserId!
        .Where(p => p.FkUser == userId)
        .Select(p => new {
            PlaylistId = p.PlaylistId,
            PlaylistName = p.PlaylistName,
            PlaylistDescription = p.PlaylistDescription,
            PlaylistGenre = p.PlaylistGenre
        })
        .ToListAsync();

    return Results.Ok(myPlaylists);
});



// GET: Alle informatie van een specifieke gebruiker ophalen via ID
app.MapGet("/users/{id}", async (int id, MixVault db) =>
{
    var user = await db.Users
        .Where(u => u.UserId == id)
        .Select(u => new
        {
            u.UserId,
            u.DisplayName,
            u.Email,
            u.ProfilePictureUrl,
            u.UserCreatedAt
        })
        .FirstOrDefaultAsync();

    if (user == null)
        return Results.NotFound($"Gebruiker met ID {id} is niet gevonden.");

    return Results.Ok(user);
});

// POST: Een fysiek audiobestand uploaden naar de server
app.MapPost("/tracks/upload", async (IFormFile file) =>
{
    // 1. Check of er daadwerkelijk een bestand is meegestuurd
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Geen bestand geüpload of bestand is leeg.");
    }

    // 2. Bepaal de map waar we het opslaan (wwwroot/uploads/tracks)
    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");

    // Maak de map aan als deze nog niet bestaat
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }

    // 3. Maak een unieke bestandsnaam (zodat "mix.mp3" niet per ongeluk een andere "mix.mp3" overschrijft)
    var fileExtension = Path.GetExtension(file.FileName); // Haalt bijv. ".mp3" op
    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}"; // Wordt bijv. "5e8...-....mp3"
    var filePath = Path.Combine(uploadPath, uniqueFileName);

    // 4. Sla het bestand daadwerkelijk op de hardeschijf op
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // 5. Geef de relatieve URL terug zodat de Frontend dit in de database kan opslaan
    var fileUrl = $"/uploads/tracks/{uniqueFileName}";

    return Results.Ok(new { FileUrl = fileUrl });
})
.DisableAntiforgery(); // Dit is nodig in .NET 8 om via Minimal APIs bestanden (IFormFile) te kunnen ontvangen

// GET: Stream het audiobestand rechtstreeks naar de browser
app.MapGet("/tracks/stream/{fileName}", async (string fileName, MixVault db) =>
{
    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
    var filePath = Path.Combine(uploadPath, fileName);

    if (!File.Exists(filePath))
    {
        return Results.NotFound("Bestand niet gevonden op de server.");
    }

    var contentType = fileName.EndsWith(".wav") ? "audio/wav" : "audio/mpeg";

    // Open het bestand en stream het met range processing (nodig voor skippen/spoelen in audiobalken!)
    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    return Results.File(fileStream, contentType, enableRangeProcessing: true);
});

// POST: Maak een gloednieuw account aan via een JSON-body
app.MapPost("/users/register", async (RegisterModel model, MixVault db) =>
{
// Check of de gebruikersnaam al bezet is
var exists = await db.Users.AnyAsync(u => u.DisplayName == model.Username);
if (exists)
{
return Results.Conflict("Deze gebruikersnaam is al in gebruik.");
}

// Maak de nieuwe gebruiker aan (We matchen jouw User model properties!)
var newUser = new User
{
DisplayName = model.Username,
Password = model.Password,
UserCreatedAt = DateTime.Now // Optioneel, mocht je dit bijhouden
};

db.Users.Add(newUser);
await db.SaveChangesAsync();

return Results.Created($"/users/{newUser.UserId}", new { IdUser = newUser.UserId, NameUser = newUser.DisplayName });
});

// POST: Een nummer toevoegen aan een specifieke afspeellijst
app.MapPost("/playlists/{playlistId}/tracks/{trackId}", async (int playlistId, int trackId, MixVault db) =>
{
    // Trucje: we zoeken eerst op wat de hoogste 'Position' in de huidige afspeellijst is.
    // Als de lijst nog leeg is, beginnen we op positie 0.
    var currentMaxPosition = await db.Playlisthastracks
        .Where(p => p.FkPlaylist == playlistId)
        .MaxAsync(p => (int?)p.Position) ?? 0;

    // Maak de koppeling aan en zet het nieuwe nummer achteraan (positie + 1)
    var newEntry = new Playlisthastrack
    {
        FkPlaylist = playlistId,
        FkTrack = trackId,
        Position = currentMaxPosition + 1
    };

    db.Playlisthastracks.Add(newEntry);
    await db.SaveChangesAsync();

    return Results.Ok(new { Bericht = "Nummer toegevoegd!", Positie = newEntry.Position });
});

app.Run();
public class RegisterModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}