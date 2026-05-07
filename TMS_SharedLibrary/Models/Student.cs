using System;
using System.Collections.Generic;

namespace TMS_SharedLibrary.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string? Placement { get; set; }

    public int? Year { get; set; }

    public virtual ICollection<ToyLoan> ToyLoans { get; set; } = new List<ToyLoan>();

    public virtual User User { get; set; } = null!;
}
