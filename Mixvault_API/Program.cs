using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mixvault_API.Models;


var builder = WebApplication.CreateBuilder(args);

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

// GET: Alle nummers (tracks) ophalen
app.MapGet("/tracks", async (MixVault db) =>
{
    var tracks = await db.Tracks
        .Select(t => new
        {
            t.TrackId,
            t.Title,
            t.DurationMs,
            t.TrackUploadedAt
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

// POST: Een nieuw nummer uploaden (alleen de data, mp3 doen we later)
app.MapPost("/tracks", async (Track newTrack, MixVault db) =>
{
    // We stellen automatisch de datum en tijd van uploaden in
    newTrack.TrackUploadedAt = DateTime.Now;

    db.Tracks.Add(newTrack);
    await db.SaveChangesAsync();

    // We sturen een bericht terug dat het succesvol is, samen met de nieuwe TrackID
    return Results.Created($"/tracks/{newTrack.TrackId}", newTrack);
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

//GET; Alle gelikete nummers van een specifieke gebruiker ophalen
app.MapGet("/users/{userId}/likes/tracks", async (int userId, MixVault db) =>
{
    var likedTracks = await db.Userlikestracks
        .Where(ult => ult.FkUser == userId)
        .Include(ult => ult.FkTrackNavigation) // Haal de gegevens van het nummer zelf op
        .Select(ult => new
        {
            ult.FkTrackNavigation.TrackId,
            ult.FkTrackNavigation.Title,
            ult.FkTrackNavigation.DurationMs,
            ult.FkTrackNavigation.TrackUploadedAt
        })
        .ToListAsync();
    return Results.Ok(likedTracks);
});

//GET: Alle gelikte afspeellijsten van een specifieke gebruiker ophalen
app.MapGet("/users/{userId}/likes/playlists", async (int userId, MixVault db) =>
{
    var likedPlaylists = await db.Userlikesplaylists
        .Where(ulp => ulp.FkUser == userId)
        .Include(ulp => ulp.FkPlaylistNavigation) // Haal de gegevens van de afspeellijst zelf op
        .Select(ulp => new
        {
            ulp.FkPlaylistNavigation.PlaylistId,
            ulp.FkPlaylistNavigation.PlaylistName,
            ulp.FkPlaylistNavigation.PlaylistDescription,
            ulp.FkPlaylistNavigation.PlaylistGenre,
            ulp.FkPlaylistNavigation.PlaylistTags
        })
        .ToListAsync();
    return Results.Ok(likedPlaylists);
});

//Post: maak een ACC aan
app.MapPost("/users/register", async (string username, string password, MixVault db) =>
{
    var exists = await db.Users
    .AnyAsync(pbl => pbl.DisplayName == username);
    if (exists)
        return Results.Conflict("Acount already in bucket list");
    //zet in databank
    var User = new User { DisplayName = username, Password = password };

    db.Users.Add(User);
    await db.SaveChangesAsync();


    return Results.Created($"/users/register", User);
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

app.Run();
