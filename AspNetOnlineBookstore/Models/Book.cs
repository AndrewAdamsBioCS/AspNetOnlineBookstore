using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetOnlineBookstore.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? BookName { get; set; }
        [Required, MaxLength(100)]
        public string? AuthorName { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string? Image { get; set; }
        [Required]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
        public List<CartDetail> CartDetail { get; set; }

        [NotMapped]
        public string GenreName { get; set; }
    }
}
