using System.ComponentModel.DataAnnotations;

namespace PaySkyDemo.DTOs
{
    public class UpdateVacancyDto
    {
        [Required]
        public string VacancyId { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "ExpiryDate is required")]
        public string ExpiryDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxApplications must be greater than 0")]
        public int MaxApplications { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
