using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySkyDemo.Controllers.Auth;
using PaySkyDemo.DTOs;
using PaySkyDemo.Services;
using PaySkyDemo.Types;
using System.ComponentModel.DataAnnotations;

namespace PaySkyDemo.Controllers.Employer
{
    public class ManageVacanciesController : ControllerBase
    {
        private readonly VacanciesService _vacanyService;
        private readonly ILogger<RegistrationController> _logger;
        public ManageVacanciesController(VacanciesService VacancyService, ILogger<RegistrationController> logger)
        {
            _vacanyService = VacancyService;
            _logger = logger;
        }

        public class DeactivateVacancyRequest
        {
            [Required]
            public int? VacancyId { get; set; }
        }

        [Authorize(Roles = "Employer")]
        [HttpGet]
        [Route("/api/Employer/Vacancies")]
        public async Task<IActionResult> GetEmployerVacanies()
        {
            var employerIdClaim = HttpContext.User.FindFirst("UserId");

            if (employerIdClaim == null || !int.TryParse(employerIdClaim.Value, out var employerId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error getting Vacancies",
                    StatusCode = 400
                });
            }
            _vacanyService.DeactivateExpiredVacancies();
            var result = await _vacanyService.GetEmployerVacancies(employerId);
            return Ok( new ApiResponseDataWithMessage { Message = "Vacancies retrieved successfully", Data = result });
        }
        [Authorize(Roles = "Employer")]
        [HttpGet]
        [Route("/api/Employer/ArchievedVacancies")]
        public async Task<IActionResult> GetEmployerArchievedVacanies()
        {
            var employerIdClaim = HttpContext.User.FindFirst("UserId");

            if (employerIdClaim == null || !int.TryParse(employerIdClaim.Value, out var employerId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error getting Vacancies",
                    StatusCode = 400
                });
            }
            _vacanyService.DeactivateExpiredVacancies();
            var result = await _vacanyService.GetArchivedVacancies(employerId);
            return Ok(new ApiResponseDataWithMessage { Message = "Vacancies retrieved successfully", Data = result });
        }
        [Authorize(Roles = "Employer")]
        [HttpPost]
        [Route("/api/Employer/DeactivateVacancy")]
        public async Task<IActionResult> DeactivateVacany([FromBody] DeactivateVacancyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var VacancyId = request.VacancyId ?? 0;
            var result = await _vacanyService.DeactivateVacany(VacancyId);
            return Ok(new ApiResponseWithMessage { Message = "Vacancy deactivated successfully" });
        }

        [Authorize(Roles = "Employer")]
        [HttpPost]
        [Route("/api/Employer/AddVacancy")]
        public async Task<IActionResult> AddVacancy([FromBody] AddVacancyRequestDto vacancy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var employerIdClaim = HttpContext.User.FindFirst("UserId");

                if (employerIdClaim == null || !int.TryParse(employerIdClaim.Value, out var employerId))
                {
                    return BadRequest(new ApiResponseWithMessage
                    {
                        Message = "Error storing Vacany",
                        StatusCode = 400
                    });
                }

                var newVacancy = new PaySkyDemo.Models.Vacancy
                {
                    Title = vacancy.Title,
                    Description = vacancy.Description,
                    ExpiryDate = DateTime.Parse(vacancy.ExpiryDate),
                    MaxApplications = vacancy.MaxApplications,
                    EmployerId = employerId,
                    IsActive = true,
                };
                var result = await _vacanyService.StoreNewVacancy(newVacancy);
                if (result)
                {
                    return Ok(new ApiResponseWithMessage { Message = "Vacancy added successfully" });
                }
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error while adding vacancy",
                    StatusCode = 400,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during creating vacancy");
                return BadRequest(new ApiResponseDataWithMessage
                {
                    Message = "Error while adding vacancy",
                    StatusCode = 400,
                    Data = new { Error = ex.Message }
                });
            }
          
        }
        [Authorize(Roles = "Employer")]
        [HttpPut]
        [Route("/api/Employer/UpdateVacancy")]
        public async Task<IActionResult> UpdateVacancy([FromBody] UpdateVacancyDto vacancy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _vacanyService.UpdateVacancy(vacancy);
                if (result)
                {
                    return Ok(new ApiResponseWithMessage { Message = "Vacancy Updated Successfully" });
                }
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error while Updating vacancy",
                    StatusCode = 400,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during updating vacancy");
                return BadRequest(new ApiResponseDataWithMessage
                {
                    Message = "Error while updaing vacancy",
                    StatusCode = 400,
                    Data = new { Error = ex.Message }
                });
            }

        }
        [Authorize(Roles = "Employer")]
        [HttpDelete("/api/Employer/Vacancy/{VacancyId}")]
        public async Task<IActionResult> DeleteVacany(int VacancyId)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var Vacany = await _vacanyService.GetVacancyById(VacancyId);
            if(Vacany == null)
            {
                return BadRequest(new ApiResponseWithMessage { Message = "Vacancy not found", StatusCode = 400 });
            }
            var result = await _vacanyService.DeleteVacancy(VacancyId);
            if (result)
            {
                return Ok(new ApiResponseWithMessage { Message = "Vacancy deleted successfully" });
            }
            return BadRequest(new ApiResponseWithMessage { Message = "Failed to delete Vacany", StatusCode = 400 });
        }




    }
}
