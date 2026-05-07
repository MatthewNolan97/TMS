namespace TMS_API.Models.DTO
{
    public class ToyLoanDTO
    {
        public string Name { get; set; }
        public int LoanId { get; set; }

        public int ToyId { get; set; }

        public int StudentId { get; set; }

        public DateOnly BorrowDate { get; set; }

        public DateOnly DueDate { get; set; }

        public DateOnly? ReturnDate { get; set; }
    }
}
