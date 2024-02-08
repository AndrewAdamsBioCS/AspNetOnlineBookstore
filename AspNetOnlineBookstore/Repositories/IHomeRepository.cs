namespace AspNetOnlineBookstore
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string searchTerm = "", int genreId = 0);

        Task<IEnumerable<Genre>> GetGenres();
    }
}