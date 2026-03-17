using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;

public partial class Playlist
{
    public int PlaylistId { get; set; }

    public string? PlaylistName { get; set; }

    public string? PlaylistDescription { get; set; }

    public string? PlaylistGenre { get; set; }

    public string? PlaylistTags { get; set; }

    public string? PlaylistArtworkUrl { get; set; }

    public DateTime? PlaylistCreatedAt { get; set; }

    public int FkUser { get; set; }

    public virtual User FkUserNavigation { get; set; } = null!;

    public virtual ICollection<Playlisthastrack> Playlisthastracks { get; set; } = new List<Playlisthastrack>();

    public virtual ICollection<Userlikesplaylist> Userlikesplaylists { get; set; } = new List<Userlikesplaylist>();
}
