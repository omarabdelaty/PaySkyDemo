namespace PaySkyDemo.DTOs
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int VacancyId { get; set; }
        public DateTime ApplicationDate { get; set; }

        public UserDto Applicant { get; set; }
        public VacancyDto Vacancy { get; set; }
    }
}
