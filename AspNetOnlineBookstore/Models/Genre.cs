using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetOnlineBookstore.Models
{
    [Table("Genre")]
    public class Genre
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string GenreName { get; set; }
        public List<Book> Books { get; set; }
    }
}
