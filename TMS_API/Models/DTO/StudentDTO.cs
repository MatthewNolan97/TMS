namespace TMS_API.Models.DTO
{
    public class StudentDTO
    {
        public int StudentId { get; set; }

        public string? Placement { get; set; }

        public int? Year { get; set; }

        public string? OID { get; set; }

        public List<string>? BorrowedToysHistory { get; set; }
        public List<string?> CurrentBorrowedToys { get; set; }

    }
}
