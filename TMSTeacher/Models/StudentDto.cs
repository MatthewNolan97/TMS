namespace TMSTeacher.Models
{
    public class StudentDto
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public string? Placement { get; set; }
        public int? Year { get; set; }
        public string? OID { get; set; }
        public List<string>? BorrowedToys { get; set; }
    }
}
