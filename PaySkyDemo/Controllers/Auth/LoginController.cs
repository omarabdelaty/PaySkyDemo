using Microsoft.AspNetCore.Mvc;
using PaySkyDemo.DTOs;
using PaySkyDemo.Models;
using PaySkyDemo.Services;
using PaySkyDemo.Types;

namespace PaySkyDemo.Controllers.Auth
{
    public class LoginController : ControllerBase
    {

        private readonly AuthService _authService;
        private readonly ILogger<RegistrationController> _logger;
        public LoginController(AuthService AuthService, ILogger<RegistrationController> logger)
        {
            _authService = AuthService;
            _logger = logger;
        }
        [HttpPost]
        [Route("/api/login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await _authService.GetUserByEmail(request.Email);

                if (user == null)
                {
                    return BadRequest(new ApiResponseWithMessage
                    {
                        Message = "these credentials do not exisit",
                        StatusCode = 400,
                    });
                }

                var result = _authService.CheckUserPassword(user.Password, request.Password);

                if (!result)
                {
                    return BadRequest(new ApiResponseWithMessage
                    {
                        Message = "these credentials do not exisit",
                        StatusCode = 400,
                    });
                }
                var token = _authService.CreateToken(user);


                return Ok(new ApiResponseDataWithMessage { Message = "Login successful", Data = new { AccessToken = token } });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user logging in");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
