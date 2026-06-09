using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCityParking.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required]
        public int ParkingSlotId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        public string VehicleNumber { get; set; } = "";

        [ForeignKey("ParkingSlotId")]
        public virtual ParkingSlot? ParkingSlot { get; set; }
        public string UserEmail { get; set; }
        public string BookingStatus
        {
            get; set;
        }
    }
}