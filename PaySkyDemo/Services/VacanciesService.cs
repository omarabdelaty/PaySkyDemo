using Microsoft.EntityFrameworkCore;
using PaySkyDemo.DTOs;
using PaySkyDemo.Models;
using System.Linq;
namespace PaySkyDemo.Services
{
    public class VacanciesService
    {
        private readonly PaySkyDemoContext _paySkyDemoContext;
        private IConfiguration _config;
        private readonly ILogger<VacanciesService> _logger;
        public VacanciesService(PaySkyDemoContext paySkyDemoContext, IConfiguration config, ILogger<VacanciesService> logger)
        {
            _paySkyDemoContext = paySkyDemoContext;
            _config = config;
            _logger = logger;
        }

        public async Task<List<Vacancy>> GetVacancies()
        {
            return await _paySkyDemoContext.Vacancies.Where(v => v.IsActive == true).ToListAsync();
        }
        public async Task<List<Vacancy>> GetArchivedVacancies(int employerId)
        {
            return await _paySkyDemoContext.Vacancies.Where(v => v.IsActive == false && v.EmployerId == employerId).ToListAsync();
        }

        public async Task<Vacancy?> GetVacancyById(int id)
        {
            if(id > 0)return await _paySkyDemoContext.Vacancies.FirstOrDefaultAsync(v => v.Id == id);

            return null;
        }
        public async Task<Vacancy[]> GetVacanciesWithFilter(string searchQuery, int? vacancyId, int? employerId, string fromDate, string toDate)
        {
            IQueryable<Vacancy> query = _paySkyDemoContext.Vacancies.Where(v => v.IsActive == true);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(v => v.Title.ToUpper().Contains(searchQuery.ToUpper()));
            }

            if (vacancyId.HasValue && vacancyId.Value > 0)
            {
                query = query.Where(v => v.Id == vacancyId.Value);
            }

            if (employerId.HasValue && employerId.Value > 0)
            {
                query = query.Where(v => v.EmployerId == employerId.Value);
            }

            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var fromDateValue))
            {
                query = query.Where(v => v.CreatedAt >= fromDateValue);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var toDateValue))
            {
                toDateValue = toDateValue.AddDays(1);
                query = query.Where(v => v.CreatedAt < toDateValue);
            }

            return await query.ToArrayAsync();
        }


        public async Task<bool> StoreNewVacancy(Vacancy newVacancy)
        {
            try
            {
                _paySkyDemoContext.Vacancies.Add(newVacancy);
                await _paySkyDemoContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while storing new vacancy");
                return false;
            }
        }

        public async Task<bool> UpdateVacancy(UpdateVacancyDto vacancyDto)
        {
            try
            {
                var existingVacancy = await _paySkyDemoContext.Vacancies.FindAsync(int.Parse(vacancyDto.VacancyId));

                if (existingVacancy == null)
                {
                    return false;
                }

                existingVacancy.Title = vacancyDto.Title;
                existingVacancy.Description = vacancyDto.Description;
                existingVacancy.ExpiryDate = DateTime.Parse(vacancyDto.ExpiryDate);
                existingVacancy.MaxApplications = vacancyDto.MaxApplications;
                existingVacancy.IsActive = vacancyDto.IsActive;

                await _paySkyDemoContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating vacancy");
                return false;
            }
        }


        public async Task<bool> DeactivateVacany(int id)
        {
            try
            {
                var vacancy = await _paySkyDemoContext.Vacancies.FirstOrDefaultAsync(v => v.Id == id);
                if (vacancy == null)
                {
                    return false;
                }
                vacancy.IsActive = false;
                await _paySkyDemoContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting vacancy");
                return false;
            }
        }
        public async Task<bool> DeleteVacancy(int id)
        {
            using (var transaction = _paySkyDemoContext.Database.BeginTransaction())
            {
                try
                {
                    var vacancy = await _paySkyDemoContext.Vacancies.FindAsync(id);

                    if (vacancy != null)
                    {
                        var relatedApplications = _paySkyDemoContext.Applications.Where(a => a.VacancyId == id).ToList();

                        foreach (var application in relatedApplications)
                        {
                            _paySkyDemoContext.Applications.Remove(application);
                        }

                        _paySkyDemoContext.Vacancies.Remove(vacancy);

                        await _paySkyDemoContext.SaveChangesAsync();

                        transaction.Commit();

                        return true;
                    }

                    transaction.Rollback();

                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while deleting vacancy");

                    transaction.Rollback();

                    return false;
                }
            }
        }

        public async Task<List<Vacancy>> GetEmployerVacancies(int EmployerId)
        {
            return await _paySkyDemoContext.Vacancies.Where(v => v.EmployerId == EmployerId).ToListAsync();
        }

        public  void DeactivateExpiredVacancies ()
        {
            _paySkyDemoContext.Vacancies.Where(v => v.ExpiryDate < DateTime.Now).ToList().ForEach(v => v.IsActive = false);
        }

        public bool CheckIfVacancyIsActive(int id)
        {
            var vacancy =  _paySkyDemoContext.Vacancies.FirstOrDefault(v => v.Id == id);

            return vacancy != null && vacancy.IsActive;
        }

    }
}
