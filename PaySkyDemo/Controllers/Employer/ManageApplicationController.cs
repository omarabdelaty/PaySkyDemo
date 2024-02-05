using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySkyDemo.Controllers.Auth;
using PaySkyDemo.Models;
using PaySkyDemo.Services;
using PaySkyDemo.Types;
using System.ComponentModel.DataAnnotations;

namespace PaySkyDemo.Controllers.Employer
{
    public class ManageApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;
        private readonly ILogger<RegistrationController> _logger;
        public ManageApplicationController(ApplicationService ApplicationService, ILogger<RegistrationController> logger)
        {
            _applicationService = ApplicationService;
            _logger = logger;
        }
        public class GetApplicationsInVacanyRequest
        {
            [Required]
            public int VacancyId { get; set; }
        }

        [Authorize(Roles = "Employer")]
        [HttpPost]
        [Route("/api/Vacancy/GetApplications")]
        public async Task<IActionResult> GetApplicationsInVacany([FromBody] GetApplicationsInVacanyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var VacancyId = request.VacancyId;
            var result = await _applicationService.GetApplications(VacancyId);
            if(result == null)
                return Ok(new ApiResponseWithMessage
                {
                    Message = "No Applications found",
                    StatusCode = 200
                });
            return Ok(new ApiResponseDataWithMessage { Message = "Vacancies retrieved successfully", Data = result });
        }
    }
}
