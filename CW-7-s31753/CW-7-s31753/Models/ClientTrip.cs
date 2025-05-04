namespace CW_7_s31753.Models
{ 
    public class ClientTrip
    {
        public int ClientId { get; set; }
        public int TripId { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string TripName { get; set; } = string.Empty;
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
    }
}