using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;

public partial class Userlikesplaylist
{
    public int UserLikesPlaylistId { get; set; }

    public int FkUser { get; set; }

    public int FkPlaylist { get; set; }

    public virtual Playlist FkPlaylistNavigation { get; set; } = null!;

    public virtual User FkUserNavigation { get; set; } = null!;
}
