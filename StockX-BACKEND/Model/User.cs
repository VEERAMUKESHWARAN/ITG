using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StockX.Model
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; } // Auto-incremented by the database
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        [Key]
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        public string Gender { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } 
    }
}
