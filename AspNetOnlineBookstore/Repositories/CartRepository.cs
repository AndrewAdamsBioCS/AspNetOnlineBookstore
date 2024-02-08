using AspNetOnlineBookstore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AspNetOnlineBookstore.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int bookId, int quantity)
        {
            string userId = GetUserId();

            // Begin unit of work
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var cart = await GetCart(userId);

                // If user's cart is null, create new cart and add to ShoppingCarts db
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _dbContext.ShoppingCarts.Add(cart);
                }
                _dbContext.SaveChanges();

                // Add cart details
                var cartItem = _dbContext.CartDetails.FirstOrDefault(i => i.ShoppingCartId == cart.Id && i.BookId == bookId);
                // If item already exists in cart, update quantity; else create new CartDetail for item
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                } else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = quantity
                    };
                    _dbContext.CartDetails.Add(cartItem);
                }
                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {

            }

            return await GetCartItemCount(userId);
        }

        public async Task<int> RemoveItem(int bookId, int quantity)
        {
            string userId = GetUserId();
            try
            {

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User not logged in");

                var cart = await GetCart(userId);
                if (cart == null)
                    throw new Exception("Invalid cart");

                // Add cart details
                var cartItem = _dbContext.CartDetails.FirstOrDefault(i => i.ShoppingCartId == cart.Id && i.BookId == bookId);
                if (cartItem == null)
                    throw new Exception("Cart empty");
                else if (cartItem.Quantity == quantity)
                    _dbContext.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity -= quantity;

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
            }

            return await GetCartItemCount(userId);
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            //if (userId == null)
            //    throw new Exception("Invalid userId");
            var shoppingCart = await _dbContext.ShoppingCarts
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genre)
                .Where(a => a.UserId == userId).FirstOrDefaultAsync();

            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            return await _dbContext.ShoppingCarts.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _dbContext.ShoppingCarts
                              where cart.UserId == userId
                              join cartDetail in _dbContext.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }
                              ).ToListAsync();
            return data.Count;
        }

        public async Task<int> MergeCarts()
        {
            // If not previously logged in, merge carts if userId differs from session Id, as this means that the user has logged in
            var userId = GetUserId();
            var sessionId = _httpContextAccessor.HttpContext.Session.Id;
            if (userId != sessionId)
            {
                var guestCart = await GetCart(sessionId);
                if (guestCart != null)
                {
                    var prevCart = await _dbContext.ShoppingCarts
                       .Include(a => a.CartDetails)
                       .Where(a => a.Id == guestCart.Id).FirstOrDefaultAsync();

                    foreach (CartDetail cartDetail in prevCart.CartDetails)
                    {
                        await AddItem(cartDetail.BookId, cartDetail.Quantity);
                        
                    }

                    // Remove guest cart details, and delete guest cart
                    using var transaction = _dbContext.Database.BeginTransaction();
                    _dbContext.CartDetails.RemoveRange(prevCart.CartDetails);
                    _dbContext.ShoppingCarts.Remove(guestCart);

                    _dbContext.SaveChanges();
                    transaction.Commit();
                }
            }

            return await GetCartItemCount(userId);
        }

        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(user);

            if (userId == null)
            {
                userId = _httpContextAccessor.HttpContext.Session.Id;
            }

            return userId;
        }

        private string GetId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(user);
            return userId;
        }
    }
}
