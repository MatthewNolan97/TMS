using System;

using System.Collections.Generic;
using TMS_SharedLibrary.Models;

namespace TMS_SharedLibrary.Models;

public partial class User

{

    public int UserId { get; set; }

    public string UserType { get; set; } = null!;

    public string? Oid { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }

}

