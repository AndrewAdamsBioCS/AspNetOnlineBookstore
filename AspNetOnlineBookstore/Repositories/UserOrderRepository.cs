using AspNetOnlineBookstore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

namespace AspNetOnlineBookstore.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserOrderRepository(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ShoppingCart> ReviewOrder()
        {
            try
            {
                var userId = GetUserId();

                var cart = await _dbContext.ShoppingCarts
                    .Include(a => a.CartDetails)
                    .ThenInclude(a => a.Book)
                    .ThenInclude(a => a.Genre)
                    .Where(a => a.UserId == userId).FirstOrDefaultAsync();

                if (cart == null)
                    throw new Exception("Invalid cart");

                var cartDetails = _dbContext.CartDetails
                    .Where(a => a.ShoppingCartId == cart.Id).ToList();

                // If cart is empty, return null cart; this happens if user nagivates back to Check Out page after placing order
                if (cartDetails == null || cartDetails.Count == 0)
                {
                    cart = null;
                }

                return cart;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> PlaceOrder()
        {
            var userId = GetUserId();

            var cart = await _dbContext.ShoppingCarts.FirstOrDefaultAsync(u => u.UserId == userId);
            if (cart == null)
                throw new Exception("Invalid cart");
            var cartDetails = _dbContext.CartDetails
                    .Where(a => a.ShoppingCartId == cart.Id).ToList();

            // If cart is empty, display last order; this happens if user refreshes the "Order Placed" page
            if(cart.CartDetails == null || cartDetails.Count == 0)
            {
                var maxOrderId = _dbContext.Orders.Max(a => a.Id);
                var lastOrder = await _dbContext.Orders
                    .Include(a => a.OrderStatus)
                    .Include(a => a.OrderDetails)
                    .ThenInclude(a => a.Book)
                    .ThenInclude(a => a.Genre)
                    .Where(a => a.Id == maxOrderId).FirstOrDefaultAsync();

                return lastOrder;
            }

            var order = new Order
            {
                UserId = userId,
                CreateDate = DateTime.UtcNow,
                OrderStatusId = 1 //pending
            };

            using var transaction = _dbContext.Database.BeginTransaction();
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            foreach (var item in cartDetails)
            {
                var book = _dbContext.Books.Find(item.BookId);
                var orderDetail = new OrderDetail
                {
                    BookId = item.BookId,
                    OrderId = order.Id,
                    Quantity = item.Quantity,
                    UnitPrice = book.Price
                };
                _dbContext.OrderDetails.Add(orderDetail);
            }
            _dbContext.SaveChanges();

            // Remove cart details
            _dbContext.CartDetails.RemoveRange(cartDetails);
            _dbContext.SaveChanges();
            transaction.Commit();

            var placedOrder = await _dbContext.Orders
                .Include(a => a.OrderStatus)
                .Include(a => a.OrderDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genre)
                .Where(a => a.Id == order.Id).FirstOrDefaultAsync();

            return placedOrder;
        }

        public async Task<IEnumerable<Order>> GetUserOrders()
        {
            var userId = GetUserId();
            if(string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in");
            }
            var orders = await _dbContext.Orders
                .Include(x => x.OrderStatus)
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Book)
                .ThenInclude(x => x.Genre)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return orders;
        }

        // USED IN CART REPOSITORY TOO; MAKE INTERFACE?
        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            string? userId = _userManager.GetUserId(user);
            //var test = _userManager.GetPhoneNumberAsync(user);
            return userId;
        }
    }
}
