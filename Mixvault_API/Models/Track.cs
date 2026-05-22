using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;
using System.ComponentModel.DataAnnotations.Schema;

public partial class Track
{
    public int TrackId { get; set; }

    public string? Title { get; set; }

    public double? DurationMs { get; set; }

    public string? TrackFileUrl { get; set; }

    public string? TrackGenre { get; set; }

    public string? TrackTags { get; set; }

    public string? TrackArtworkUrl { get; set; }

    public string? TrackArtist { get; set; }

    public DateTime? TrackUploadedAt { get; set; }

    public int FkUser { get; set; }

    public virtual User FkUserNavigation { get; set; } = null!;

    public virtual ICollection<Playlisthastrack> Playlisthastracks { get; set; } = new List<Playlisthastrack>();

    public virtual ICollection<Userlikestrack> Userlikestracks { get; set; } = new List<Userlikestrack>();}
