using PaySkyDemo.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PaySkyDemo.DTOs
{
    public class RegistrationRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public UserType UserType { get; set; }

    }
}
