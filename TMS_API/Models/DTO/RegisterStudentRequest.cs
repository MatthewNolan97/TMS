namespace TMS_API.Models.DTO
{
   
        public class RegisterStudentRequest
        {
            public string Oid { get; set; }
            public string Placement { get; set; }
            public int Year { get; set; } = DateTime.Now.Year;
        }
    
}
