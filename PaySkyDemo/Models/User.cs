using PaySkyDemo.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaySkyDemo.Models
{
    public class User
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }

        public required UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
