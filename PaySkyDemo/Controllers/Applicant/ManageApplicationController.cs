using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySkyDemo.Controllers.Auth;
using PaySkyDemo.Services;
using PaySkyDemo.Types;
using System.ComponentModel.DataAnnotations;

namespace PaySkyDemo.Controllers.Applicant
{
    public class ManageApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;
        private readonly VacanciesService _vacancyService;
        private readonly ILogger<RegistrationController> _logger;
        public ManageApplicationController(ApplicationService ApplicationService, ILogger<RegistrationController> logger, VacanciesService vacanciesService)
        {
            _applicationService = ApplicationService;
            _logger = logger;
            _vacancyService = vacanciesService;
        }
        public class ApplyForVacancyRequest
        {
            [Required]
            public string VacancyId { get; set; }
        }
        public class GetVacanciesWithFilterRequest
        {
            public string? QueryString { get; set; } 
            public int? VacancyId { get; set; }
            public int? EmployerId { get; set; }
            public string? fromDate { get; set; }
            public string? toDate { get; set; }

        }


        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("/api/Applicatons/Apply")]

        public async Task<IActionResult> ApplyForVacancy([FromBody] ApplyForVacancyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var UserIdClaim = HttpContext.User.FindFirst("UserId");
            var VacancyId = request.VacancyId;
            if (UserIdClaim == null || !int.TryParse(UserIdClaim.Value, out var userId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error getting Vacancies",
                    StatusCode = 400
                });
            }
            _vacancyService.DeactivateExpiredVacancies();
            var parsedVacancyId = int.Parse(VacancyId);
            if (await _vacancyService.GetVacancyById(parsedVacancyId) == null)
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Vacancy does not exist",
                    StatusCode = 400
                });
            }
            if (!_vacancyService.CheckIfVacancyIsActive(parsedVacancyId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Vacancy is not active",
                    StatusCode = 400
                });
            }
            if (!_applicationService.checkIfUserHasExceededDailyLimitOfApplications(userId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "You have exceded your daily limit for applying",
                    StatusCode = 400
                });
            }
            if (!_applicationService.checkIfUserHasAppliedToVacany(userId, parsedVacancyId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "You have already applied for this vacancy",
                    StatusCode = 400
                });
            }
            if (_applicationService.checkIfVacancyHasExceededLimit(parsedVacancyId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "The Vacany has exceded its limit for applications",
                    StatusCode = 400
                });
            }
            var result = await _applicationService.StoreNewApplication(userId, parsedVacancyId);
            if (!result)
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error applying for vacancy",
                    StatusCode = 400
                });
            }
            return Ok(new ApiResponseWithMessage { Message = "Application submitted successfully" });
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("/api/Applications/Get")]
        public async Task<IActionResult> GetUserApplications()
        {
            var UserIdClaim = HttpContext.User.FindFirst("UserId");

            if (UserIdClaim == null || !int.TryParse(UserIdClaim.Value, out var userId))
            {
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error getting Vacancies",
                    StatusCode = 400
                });
            }
            var result = await _applicationService.GetApplications(0,userId);
            if (result == null)
                return Ok(new ApiResponseWithMessage
                {
                    Message = "No Applications found",
                    StatusCode = 200
                });
            return Ok(new ApiResponseDataWithMessage { Message = "Applications retrieved successfully", Data = result });
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("/api/Applicatons/Search")]
        public async Task<IActionResult> GetVacanciesWithFilter([FromBody]GetVacanciesWithFilterRequest request)
        {
            _vacancyService.DeactivateExpiredVacancies();
            var result = await _vacancyService.GetVacanciesWithFilter(request.QueryString ?? "", request.VacancyId ?? 0, request.EmployerId ?? 0,request.fromDate ?? "",request.toDate ?? "");
            if (result == null)
                return Ok(new ApiResponseWithMessage
                {
                    Message = "No Applications found",
                    StatusCode = 200
                });
             
            return Ok(new ApiResponseDataWithMessage { Message = "Applications retrieved successfully", Data = result });
        }
    }
}
