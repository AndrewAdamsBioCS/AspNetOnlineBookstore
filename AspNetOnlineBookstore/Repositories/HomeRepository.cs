
using Microsoft.EntityFrameworkCore;

namespace AspNetOnlineBookstore.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HomeRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Genre>> GetGenres()
        {
            return await _dbContext.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string searchTerm="", int genreId = 0)
        {
            if(!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
            }            

            IEnumerable<Book> books = await (from book in _dbContext.Books
                         join genre in _dbContext.Genres
                         on book.GenreId equals genre.Id
                         where string.IsNullOrWhiteSpace(searchTerm) || (book != null && book.BookName.ToLower().Contains(searchTerm))

                         select new Book
                         {
                             Id = book.Id,
                             AuthorName = book.AuthorName,
                             BookName = book.BookName,
                             GenreId = book.GenreId,
                             Price = book.Price,
                             GenreName = genre.GenreName
                         }).ToListAsync();

            if(genreId > 0)
            {
                books = books.Where(b => b.GenreId ==  genreId).ToList();
            }

            return books;
        }
    }
}
