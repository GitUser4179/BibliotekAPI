using System.Text.Json.Serialization;

namespace BibliotekAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Phone { get; set; }
        [JsonIgnore]
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
