using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_SharedLibrary.Models
{
    public class ToyViewModel
    {
        public Toy Toy { get; set; } = null!;
        public ToyLoan? ToyLoan { get; set; }
        public int? DaysLeft { get; set; }
        public bool BorrowedByCurrentStudent { get; set; }
    }
}
