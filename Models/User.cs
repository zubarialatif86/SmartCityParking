using System.ComponentModel.DataAnnotations;

namespace SmartCityParking.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Smart Parking Specific Fields
        public DateTime AccountCreated { get; set; } = DateTime.Now;
        public bool IsBlocked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal WalletBalance { get; set; } = 0.00m; // Driver automatic token top-up wallet
    }

}