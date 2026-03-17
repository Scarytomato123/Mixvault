using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime? UserCreatedAt { get; set; }

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    public virtual ICollection<Userlikesplaylist> Userlikesplaylists { get; set; } = new List<Userlikesplaylist>();

    public virtual ICollection<Userlikestrack> Userlikestracks { get; set; } = new List<Userlikestrack>();
}
