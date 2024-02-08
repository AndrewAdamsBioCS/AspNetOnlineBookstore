using AspNetOnlineBookstore.Models;
using AspNetOnlineBookstore.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetOnlineBookstore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _homeRepository = homeRepository;

            // Set value in session, to maintain static session ID
            var sessionId = _httpContextAccessor.HttpContext.Session.Id;
            _httpContextAccessor.HttpContext.Session.Set("currentSession", System.Text.Encoding.Unicode.GetBytes(sessionId));
        }

        public async Task<IActionResult> Index(string searchTerm="", int genreId = 0)
        {
            IEnumerable<Book> books = await _homeRepository.GetBooks(searchTerm, genreId);
            IEnumerable<Genre> genres = await _homeRepository.GetGenres();
            BookDisplayModel bookModel = new BookDisplayModel
            {
                Books = books,
                Genres = genres,
                SearchTerm = searchTerm,
                GenreId = genreId
            };

            return View(bookModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}