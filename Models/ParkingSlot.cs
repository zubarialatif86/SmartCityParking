namespace SmartCityParking.Models
{
    public class ParkingSlot
    {
        public int Id { get; set; }
        public string SlotNumber { get; set; } = "";
        public string Location { get; set; } = "";
        public string AreaName { get; set; } = "";
        public string City { get; set; } = "";
        public string VehicleType { get; set; } = "Car";
        public bool IsAvailable { get; set; } = true;
        public bool IsReserved { get; set; } = false;
        public decimal PricePerHour { get; set; } = 50;
        public string Type { get; set; }
    }
}