namespace CW_7_s31753.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxPeople { get; set; }
        public List<Country> Countries { get; set; } = new List<Country>();
    }
}