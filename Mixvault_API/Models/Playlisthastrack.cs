using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;

public partial class Playlisthastrack
{
    public int PlaylistHasTracksId { get; set; }

    public int Position { get; set; }

    public int FkTrack { get; set; }

    public int FkPlaylist { get; set; }

    public virtual Playlist FkPlaylistNavigation { get; set; } = null!;

    public virtual Track FkTrackNavigation { get; set; } = null!;
}
