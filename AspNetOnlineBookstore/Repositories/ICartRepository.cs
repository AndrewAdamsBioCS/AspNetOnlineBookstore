using Microsoft.EntityFrameworkCore;

namespace AspNetOnlineBookstore.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int quantity);
        Task<int> RemoveItem(int bookId, int quantity);
        Task<ShoppingCart> GetUserCart();
        Task<ShoppingCart> GetCart(string userId);
        Task<int> GetCartItemCount(string userId="");
        Task<int> MergeCarts();
    }
}