using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PaySkyDemo.DTOs;
using PaySkyDemo.Models;

namespace PaySkyDemo.Services
{
    public class ApplicationService
    {
        private readonly PaySkyDemoContext _paySkyDemoContext;
        private readonly ILogger<VacanciesService> _logger;
        public ApplicationService(PaySkyDemoContext paySkyDemoContext, IConfiguration config, ILogger<VacanciesService> logger)
        {
            _paySkyDemoContext = paySkyDemoContext;
            _logger = logger;

        }
        public async Task<ApplicationDto[]> GetApplications(int VacancyId = 0, int ApplicantId = 0)
        {
            var query = _paySkyDemoContext.Applications
                .Include(application => application.Applicant)
                .Include(application => application.Vacancy)
                .Join(
                    _paySkyDemoContext.Users,
                    application => application.ApplicantId,
                    user => user.ID,
                    (application, user) => new { Application = application, User = user })
                .Join(
                    _paySkyDemoContext.Vacancies,
                    joinResult => joinResult.Application.VacancyId,
                    vacancy => vacancy.Id,
                    (joinResult, vacancy) => new { Application = joinResult.Application, User = joinResult.User, Vacancy = vacancy });

            if (VacancyId != 0) { query = query.Where(joinResult => joinResult.Application.VacancyId == VacancyId); }
            if (ApplicantId > 0) { query = query.Where(joinResult => joinResult.Application.ApplicantId == ApplicantId); }

            var applications = await query
                .Select(joinResult => new ApplicationDto
                {
                    Id = joinResult.Application.Id,
                    ApplicantId = joinResult.Application.ApplicantId,
                    VacancyId = joinResult.Application.VacancyId,
                    ApplicationDate = joinResult.Application.ApplicationDate,
                    Applicant = new UserDto
                    {
                        Id = joinResult.User.ID,
                        Name = joinResult.User.Name,
                        Email = joinResult.User.Email,
                        UserType = joinResult.User.UserType.ToString()
                    },
                    Vacancy = new VacancyDto
                    {
                        Id = joinResult.Vacancy.Id,
                        EmployerId = joinResult.Vacancy.EmployerId,
                        Title = joinResult.Vacancy.Title,
                        Description = joinResult.Vacancy.Description,
                        ExpiryDate = joinResult.Vacancy.ExpiryDate,
                        MaxApplications = joinResult.Vacancy.MaxApplications,
                        IsActive = joinResult.Vacancy.IsActive
                    }
                })
                .ToArrayAsync();

            return applications;
        }


        public async Task<bool> StoreNewApplication( int UserId, int VacanyId)
        {
            try
            {
                var newApplication = new Application
                {
                    ApplicantId = UserId,
                    VacancyId = VacanyId,
                    ApplicationDate = DateTime.Now
                };
                
                _paySkyDemoContext.Applications.Add(newApplication);
                await _paySkyDemoContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while storing new Application");
                return false;
            }
        }

        public bool checkIfUserHasExceededDailyLimitOfApplications(int ApplicantId)
        {
            var checkIfUserHasExceededLimit = _paySkyDemoContext.Applications.Any(a => a.ApplicantId == ApplicantId && a.ApplicationDate >= DateTime.Now.AddHours(-24));
            if (checkIfUserHasExceededLimit) return false;
            return true;
        }
        public bool checkIfUserHasAppliedToVacany(int ApplicantId, int VacancyId)
        {
            var checkIfUserHasAppliedToThisvacany = _paySkyDemoContext.Applications.Any(a => a.ApplicantId == ApplicantId && a.VacancyId == VacancyId);
            if (checkIfUserHasAppliedToThisvacany) return false;
            return true;
        }

        public bool checkIfVacancyHasExceededLimit(int VacancyId)
        {
            return _paySkyDemoContext.Applications.Count(a => a.VacancyId == VacancyId) > _paySkyDemoContext.Vacancies.FirstOrDefault(v => v.Id == VacancyId).MaxApplications;
        }


    }
}
