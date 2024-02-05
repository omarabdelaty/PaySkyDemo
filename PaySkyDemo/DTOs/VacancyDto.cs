namespace PaySkyDemo.DTOs
{
    public class VacancyDto
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxApplications { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ApplicationDto> Applications { get; set; }
    }
}
