using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_SharedLibrary.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public int UserId { get; set; }


    public virtual ICollection<Toy> Toys { get; set; } = new List<Toy>();

    public virtual User User { get; set; } = null!;
}
