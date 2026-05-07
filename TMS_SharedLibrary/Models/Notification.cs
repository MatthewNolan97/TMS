using System;
using System.Collections.Generic;

namespace TMS_SharedLibrary.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int RecipientId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? DateSent { get; set; }

    public string? Type { get; set; }

    public virtual User Recipient { get; set; } = null!;
}
