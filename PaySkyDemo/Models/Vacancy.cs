using Microsoft.EntityFrameworkCore;

namespace PaySkyDemo.Models
{
    public class Vacancy
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxApplications { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public  User Employer { get; set; }
        public  ICollection<Application> Applications { get; set; }


     
    }

  
}
