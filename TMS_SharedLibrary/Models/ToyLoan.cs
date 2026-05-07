using System;
using System.Collections.Generic;

namespace TMS_SharedLibrary.Models;

public partial class ToyLoan
{
    public int LoanId { get; set; }

    public int ToyId { get; set; }

    public int StudentId { get; set; }
    public DateOnly BorrowDate { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Toy Toy { get; set; } = null!;
}
