namespace TMS_API.Models.DTO
{
    public class ToyDTO
    {
        public int ToyId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? Material { get; set; }

        public string? LocationCode { get; set; }

        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        public int? ManagedBy { get; set; }

        public string? AdditionalInformation { get; set; }
    }
}
