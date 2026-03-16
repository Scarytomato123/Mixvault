using Microsoft.EntityFrameworkCore;
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

//// GET: Alle bucket list items van een user (met executed status)
//app.MapGet("/users/{userId}/bucketlist", async (int userId, MixVault db) =>
//{
//    var items = await db.Personalbucketlists
//        .Where(pbl => pbl.FkUser == userId)
//        .Include(pbl => pbl.FkBucketListItemNavigation)
//        .Select(pbl => new
//        {
//            ItemId = pbl.FkBucketListItem,
//            Name = pbl.FkBucketListItemNavigation.NameBucketListItem,
//            Description = pbl.FkBucketListItemNavigation.DescriptionBucketListItem,
//            Executed = pbl.Executed
//        })
//        .ToListAsync();

//    return Results.Ok(items);
//});

//// POST: Voeg een item toe aan de bucket list van een user
//app.MapPost("/users/{userId}/bucketlist/{itemId}", async (int userId, int itemId, MixVault db) =>
//{
//    // Check of item al bestaat voor deze user
//    var exists = await db.Personalbucketlists
//        .AnyAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

//    if (exists)
//        return Results.Conflict("Item already in bucket list");

//    var personalItem = new Personalbucketlist
//    {
//        FkUser = userId,
//        FkBucketListItem = itemId,
//        Executed = false
//    };

//    db.Personalbucketlists.Add(personalItem);
//    await db.SaveChangesAsync();

//    return Results.Created($"/users/{userId}/bucketlist", personalItem);
//});

//// PUT: Markeer een item als executed/not executed
//app.MapPut("/users/{userId}/bucketlist/{itemId}/toggle", async (int userId, int itemId, MixVault db) =>
//{
//    var item = await db.Personalbucketlists
//        .FirstOrDefaultAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

//    if (item == null)
//        return Results.NotFound();

//    item.Executed = !item.Executed; // Toggle
//    await db.SaveChangesAsync();

//    return Results.Ok(new { Executed = item.Executed });
//});

//// DELETE: Verwijder een item uit de bucket list van een user
//app.MapDelete("/users/{userId}/bucketlist/{itemId}", async (int userId, int itemId, MixVault db) =>
//{
//    var item = await db.Personalbucketlists
//        .FirstOrDefaultAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

//    if (item == null)
//        return Results.NotFound();

//    db.Personalbucketlists.Remove(item);
//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});

//// GET: Alle beschikbare bucket list items
//app.MapGet("/bucketlistitems", async (MixVault db) =>
//{
//    var items = await db.Personalbucketlists.ToListAsync();
//    return Results.Ok(items);
//});

app.Run();
