using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS_SharedLibrary.Models;

public partial class Toy
{
    public int ToyId { get; set; }

    //[Required(ErrorMessage = "Toy Name is required.")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    //[Required(ErrorMessage = "Development Goal is required.")]
    public string? Category { get; set; }

    //[Required(ErrorMessage = "Material is required.")]
    public string? Material { get; set; }

    //[Required(ErrorMessage = "Location is required.")]
    public string? LocationCode { get; set; }

    public string? ImagePath { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public int? ManagedBy { get; set; }

    public string? AdditionalInformation { get; set; }
    public virtual Teacher? ManagedByNavigation { get; set; }

    public virtual ICollection<ToyLoan> ToyLoans { get; set; } = new List<ToyLoan>();
}
