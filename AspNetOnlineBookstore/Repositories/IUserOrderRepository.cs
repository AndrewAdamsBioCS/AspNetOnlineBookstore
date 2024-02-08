namespace AspNetOnlineBookstore.Repositories
{
    public interface IUserOrderRepository
    {
        Task<ShoppingCart> ReviewOrder();
        Task<Order> PlaceOrder();
        Task<IEnumerable<Order>> GetUserOrders();
    }
}