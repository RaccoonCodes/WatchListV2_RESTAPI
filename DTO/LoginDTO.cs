using System.ComponentModel.DataAnnotations;

namespace WatchListV2.DTO
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(255)]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? UserId { get; set; }
    }
}
