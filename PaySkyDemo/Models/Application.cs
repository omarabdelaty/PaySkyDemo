namespace PaySkyDemo.Models
{
    public class Application
    {

        public int Id { get; set; }
        public int ApplicantId { get; set; } // Foreign key to the applicant
        public int VacancyId { get; set; } // Foreign key to the vacancy
        public DateTime ApplicationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public  User Applicant { get; set; }
        public  Vacancy Vacancy { get; set; }
    }
}
