using System;
using System.Collections.Generic;

namespace Mixvault_API.Models;

public partial class Userlikestrack
{
    public int UserLikesTrackId { get; set; }

    public int FkUser { get; set; }

    public int FkTrack { get; set; }

    public virtual Track FkTrackNavigation { get; set; } = null!;

    public virtual User FkUserNavigation { get; set; } = null!;
}
