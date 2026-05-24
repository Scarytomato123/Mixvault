using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mixvault_API.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Vertel de webserver (Kestrel) dat grote bestanden (200 MB) zijn toegestaan
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 200 * 1024 * 1024;
});

var connectionString = "Server=localhost;Database=MixVault;User=root;Password=1234;";
builder.Services.AddDbContext<MixVault>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// ========================================================================
// USERS & AUTHENTICATIE
// ========================================================================

// GET: Kijk of wachtwoord en username overeenkomen (Login)
app.MapGet("/login", async (string username, string password, MixVault db) =>
{
    var user = await db.Users
        .Where(u => u.DisplayName == username && u.Password == password)
        .Select(u => new { IdUser = u.UserId, NameUser = u.DisplayName })
        .FirstOrDefaultAsync();

    if (user == null) return Results.Unauthorized();
    return Results.Ok(user);
});

// POST: Maak een gloednieuw account aan (Registreren)
app.MapPost("/users/register", async (RegisterModel model, MixVault db) =>
{
    var exists = await db.Users.AnyAsync(u => u.DisplayName == model.Username);
    if (exists) return Results.Conflict("Deze gebruikersnaam is al in gebruik.");

    var newUser = new User
    {
        DisplayName = model.Username,
        Password = model.Password,
        UserCreatedAt = DateTime.Now
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{newUser.UserId}", new { IdUser = newUser.UserId, NameUser = newUser.DisplayName });
});

// GET: Alle users
app.MapGet("/users", async (MixVault db) =>
{
    return Results.Ok(await db.Users.ToListAsync());
});

// GET: Alle informatie van een specifieke gebruiker ophalen via ID
app.MapGet("/users/{id}", async (int id, MixVault db) =>
{
    var user = await db.Users
        .Where(u => u.UserId == id)
        .Select(u => new { u.UserId, u.DisplayName, u.Email, u.ProfilePictureUrl, u.UserCreatedAt })
        .FirstOrDefaultAsync();

    if (user == null) return Results.NotFound($"Gebruiker met ID {id} is niet gevonden.");
    return Results.Ok(user);
});


// ========================================================================
// TRACKS (NUMMERS)
// ========================================================================

// GET: Alle nummers ophalen inclusief uploader naam
app.MapGet("/tracks", async (MixVault db) =>
{
    var tracks = await db.Tracks
        .Include(t => t.FkUserNavigation)
        .Select(t => new
        {
            t.TrackId,
            t.Title,
            t.TrackArtist,
            t.TrackGenre,
            t.DurationMs,
            t.TrackUploadedAt,
            t.TrackFileUrl,
            UploaderName = t.FkUserNavigation != null ? t.FkUserNavigation.DisplayName : "Onbekende DJ"
        })
        .ToListAsync();
    return Results.Ok(tracks);
});

// GET: Stream het audiobestand rechtstreeks naar de browser
app.MapGet("/tracks/stream/{fileName}", async (string fileName, MixVault db) =>
{
    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
    var filePath = Path.Combine(uploadPath, fileName);

    if (!File.Exists(filePath)) return Results.NotFound("Bestand niet gevonden op de server.");

    var contentType = fileName.EndsWith(".wav") ? "audio/wav" : "audio/mpeg";
    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    return Results.File(fileStream, contentType, enableRangeProcessing: true);
});

// POST: Nummer uploaden en fysiek + in database opslaan
app.MapPost("/tracks", async (HttpRequest request, MixVault db) =>
{
    if (!request.HasFormContentType || !request.Form.Files.Any()) return Results.BadRequest("Geen audiobestand ontvangen.");

    var file = request.Form.Files[0];
    var title = request.Form["title"].ToString();
    var artist = request.Form["artist"].ToString();
    var genre = request.Form["genre"].ToString();
    var fkUser = int.Parse(request.Form["fkUser"].ToString());

    double? durationMs = double.TryParse(request.Form["durationMs"].ToString(), out var d) ? d : null;

    if (string.IsNullOrEmpty(title)) return Results.BadRequest("Titel is verplicht.");

    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

    using (var stream = new FileStream(Path.Combine(uploadFolder, uniqueFileName), FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var newTrack = new Track
    {
        Title = title,
        TrackArtist = artist,
        TrackGenre = genre,
        FkUser = fkUser,
        DurationMs = durationMs,
        TrackUploadedAt = DateTime.Now,
        TrackFileUrl = $"/tracks/stream/{uniqueFileName}"
    };

    db.Tracks.Add(newTrack);
    await db.SaveChangesAsync();

    return Results.Created($"/tracks/{newTrack.TrackId}", newTrack);
});

// DELETE: Verwijder een track volledig van de server
app.MapDelete("/tracks/{id:int}", async (int id, MixVault db) =>
{
    var track = await db.Tracks.FindAsync(id);
    if (track == null) return Results.NotFound("Track niet gevonden.");

    if (!string.IsNullOrEmpty(track.TrackFileUrl))
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", track.TrackFileUrl.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    db.Tracks.Remove(track);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = "Track succesvol gewist." });
});


// ========================================================================
// PLAYLISTS
// ========================================================================

// GET: Alle afspeellijsten ophalen
app.MapGet("/playlists", async (MixVault db) =>
{
    var playlists = await db.Playlists
        .Select(p => new { p.PlaylistId, p.PlaylistName, p.PlaylistDescription, p.PlaylistGenre, p.PlaylistTags })
        .ToListAsync();
    return Results.Ok(playlists);
});

// GET: Alle nummers van een specifieke afspeellijst ophalen (MET TRACKURL FIX)
app.MapGet("/playlists/{id}/tracks", async (int id, MixVault db) =>
{
    var tracksInPlaylist = await db.Playlisthastracks
        .Where(pht => pht.FkPlaylist == id)
        .Include(pht => pht.FkTrackNavigation)
        .OrderBy(pht => pht.Position)
        .Select(pht => new
        {
            Position = pht.Position,
            TrackId = pht.FkTrackNavigation.TrackId,
            Title = pht.FkTrackNavigation.Title,
            Duration = pht.FkTrackNavigation.DurationMs,
            TrackFileUrl = pht.FkTrackNavigation.TrackFileUrl // <-- Zorgt dat Play knop in modal werkt!
        })
        .ToListAsync();
    return Results.Ok(tracksInPlaylist);
});

// POST: Maak een nieuwe playlist aan voor een specifieke user
app.MapPost("/users/{userId}/playlists", async (int userId, Playlist newPlaylist, MixVault db) =>
{
    // Koppel de playlist aan de maker
    newPlaylist.FkUser = userId;
    newPlaylist.PlaylistCreatedAt = DateTime.Now;

    db.Playlists.Add(newPlaylist);
    await db.SaveChangesAsync();

    // We sturen het hele object terug (inclusief de nieuw gegenereerde PlaylistId) 
    // zodat de frontend weet in welke ID de track direct daarna gestoken moet worden!
    return Results.Ok(newPlaylist);
});

// GET: Haal alle eigen gemaakte playlists van een specifieke user op
app.MapGet("/users/{userId:int}/playlists", async (int userId, MixVault db) =>
{
    var myPlaylists = await db.Playlists
        .Where(p => p.FkUser == userId)
        .Select(p => new
        {
            p.PlaylistId,
            p.PlaylistName,
            p.PlaylistDescription,
            p.PlaylistGenre,
            p.PlaylistTags
        })
        .ToListAsync();

    return Results.Ok(myPlaylists);
});

// POST: Een nieuwe afspeellijst maken
app.MapPost("/playlists", async (Playlist newPlaylist, MixVault db) =>
{
    newPlaylist.PlaylistCreatedAt = DateTime.Now;
    db.Playlists.Add(newPlaylist);
    await db.SaveChangesAsync();
    return Results.Created($"/playlists/{newPlaylist.PlaylistId}", newPlaylist);
});

// POST: Een nummer toevoegen aan een specifieke afspeellijst
app.MapPost("/playlists/{playlistId}/tracks/{trackId}", async (int playlistId, int trackId, MixVault db) =>
{
    var currentMaxPosition = await db.Playlisthastracks.Where(p => p.FkPlaylist == playlistId).MaxAsync(p => (int?)p.Position) ?? 0;

    var newEntry = new Playlisthastrack { FkPlaylist = playlistId, FkTrack = trackId, Position = currentMaxPosition + 1 };
    db.Playlisthastracks.Add(newEntry);
    await db.SaveChangesAsync();
    return Results.Ok(new { Bericht = "Nummer toegevoegd!", Positie = newEntry.Position });
});

// DELETE: Een volledige playlist wissen (Nieuw)
app.MapDelete("/playlists/{id:int}", async (int id, MixVault db) =>
{
    var playlist = await db.Playlists.FindAsync(id);
    if (playlist == null) return Results.NotFound("Playlist niet gevonden.");

    db.Playlists.Remove(playlist);
    await db.SaveChangesAsync();
    return Results.Ok("Playlist succesvol verwijderd.");
});

// DELETE: Een nummer uit een playlist verwijderen (Nieuw)
app.MapDelete("/playlists/{playlistId}/tracks/{trackId}", async (int playlistId, int trackId, MixVault db) =>
{
    var link = await db.Playlisthastracks.FirstOrDefaultAsync(p => p.FkPlaylist == playlistId && p.FkTrack == trackId);
    if (link == null) return Results.NotFound("Track zit niet in deze playlist.");

    db.Playlisthastracks.Remove(link);
    await db.SaveChangesAsync();
    return Results.Ok("Track succesvol uit playlist verwijderd.");
});


// ========================================================================
// LIKES (TRACKS & PLAYLISTS)
// ========================================================================

// GET: Alle gelikete nummers van een specifieke gebruiker ophalen
app.MapGet("/users/{userId}/likes/tracks", async (int userId, MixVault db) =>
{
    var likedTracks = await db.Userlikestracks
        .Where(u => u.FkUser == userId)
        .Include(u => u.FkTrackNavigation).ThenInclude(t => t.FkUserNavigation)
        .Select(u => new
        {
            u.FkTrackNavigation.TrackId,
            u.FkTrackNavigation.Title,
            TrackArtist = u.FkTrackNavigation.TrackArtist,
            TrackGenre = u.FkTrackNavigation.TrackGenre,
            u.FkTrackNavigation.DurationMs,
            u.FkTrackNavigation.TrackUploadedAt,
            TrackFileUrl = u.FkTrackNavigation.TrackFileUrl,
            UploaderName = u.FkTrackNavigation.FkUserNavigation != null ? u.FkTrackNavigation.FkUserNavigation.DisplayName : "Onbekende DJ"
        })
        .ToListAsync();
    return Results.Ok(likedTracks);
});

// POST: Een nummer liken
app.MapPost("/users/{userId}/likes/tracks/{trackId}", async (int userId, int trackId, MixVault db) =>
{
    var exists = await db.Userlikestracks.AnyAsync(ult => ult.FkUser == userId && ult.FkTrack == trackId);
    if (exists) return Results.Conflict("Je hebt dit nummer al geliket.");

    db.Userlikestracks.Add(new Userlikestrack { FkUser = userId, FkTrack = trackId });
    await db.SaveChangesAsync();
    return Results.Ok("Nummer geliket!");
});

// DELETE: Een like van een nummer weghalen
app.MapDelete("/users/{userId}/likes/tracks/{trackId}", async (int userId, int trackId, MixVault db) =>
{
    var like = await db.Userlikestracks.FirstOrDefaultAsync(ult => ult.FkUser == userId && ult.FkTrack == trackId);
    if (like == null) return Results.NotFound("Like niet gevonden.");

    db.Userlikestracks.Remove(like);
    await db.SaveChangesAsync();
    return Results.Ok("Like verwijderd!");
});

// GET: Alle gelikte afspeellijsten van een gebruiker
app.MapGet("/users/{userId}/likes/playlists", async (int userId, MixVault db) =>
{
    var likedPlaylists = await db.Userlikesplaylists
        .Where(u => u.FkUser == userId)
        .Include(u => u.FkPlaylistNavigation)
        .Select(u => new
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

// POST: Een playlist liken (Ontvolgen / Parameter typos gefixt!)
app.MapPost("/users/{userId}/likes/playlist/{playlistId}", async (int userId, int playlistId, MixVault db) =>
{
    var exists = await db.Userlikesplaylists.AnyAsync(ult => ult.FkUser == userId && ult.FkPlaylist == playlistId);
    if (exists) return Results.Conflict("Je hebt deze playlist al geliket.");

    db.Userlikesplaylists.Add(new Userlikesplaylist { FkUser = userId, FkPlaylist = playlistId });
    await db.SaveChangesAsync();
    return Results.Ok("Playlist geliket!");
});

// DELETE: Een playlist ontvolgen (Parameter typos gefixt!)
app.MapDelete("/users/{userId}/likes/playlist/{playlistId}", async (int userId, int playlistId, MixVault db) =>
{
    var like = await db.Userlikesplaylists.FirstOrDefaultAsync(ult => ult.FkUser == userId && ult.FkPlaylist == playlistId);
    if (like == null) return Results.NotFound("Playlist niet gevonden.");

    db.Userlikesplaylists.Remove(like);
    await db.SaveChangesAsync();
    return Results.Ok("Playlist ontvolgd!");
});

app.Run();

// ========================================================================
// EXTRA KLASSEN (MODELS)
// ========================================================================
public class RegisterModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}