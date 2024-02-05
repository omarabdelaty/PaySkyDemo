using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaySkyDemo.DTOs;
using PaySkyDemo.Models;
using PaySkyDemo.Services;
using PaySkyDemo.Types;
using System.Data;
using System.Text.RegularExpressions;

namespace PaySkyDemo.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {


        private readonly AuthService _authService;
        private readonly ILogger<RegistrationController> _logger;
        public RegistrationController(AuthService authService, ILogger<RegistrationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await _authService.GetUserByEmail(req.Email);

                if (user != null)
                {
                    return BadRequest(new ApiResponseWithMessage
                    {
                        Message = "Email registered before",
                        StatusCode = 400,
                    });
                }
                string passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

                if (!Regex.IsMatch(req.Password, passwordRegex))
                {
                    return BadRequest(new ApiResponseWithMessage
                    {
                        Message = "The Password must contain at least one upper case, number,special charecter, and minum 8 charecters",
                        StatusCode = 400,
                    });
                }
                var newUser = new User
                {
                    Name = req.Name,
                    Email = req.Email,
                    Password = req.Password,
                    UserType = req.UserType
                };

                var result = await _authService.StoreNewUser(newUser);

                if (result)
                {
                    _logger.LogInformation("User Created");
                    return Ok(new ApiResponseWithMessage { Message = "user registered succefully" });
                }

                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error creating user",
                    StatusCode = 400,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration");
                return BadRequest(new ApiResponseWithMessage
                {
                    Message = "Error creating user",
                    StatusCode = 400,
                });
            }
        }
    }
}
