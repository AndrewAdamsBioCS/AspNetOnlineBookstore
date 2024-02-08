using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetOnlineBookstore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<IActionResult> AddItem(int bookId, int quantity = 1, int redirect = 0)
        {
            var cartCount = await _cartRepository.AddItem(bookId, quantity);
            if (redirect > 0)
            {
                return RedirectToAction("GetUserCart");
            }
            return Ok(cartCount);
        }

        public async Task<IActionResult> RemoveItem(int bookId, int quantity = 1)
        {
            var cartCount = await _cartRepository.RemoveItem(bookId, quantity);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepository.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetCartItemCount()
        {
            int cartItemCount = await _cartRepository.GetCartItemCount();
            return Ok(cartItemCount);
        }

        public async Task<IActionResult> ChangeGuestToUser()
        {
            int mergedCartItemCount = await _cartRepository.MergeCarts();
            return Ok(mergedCartItemCount);
        }

        [Authorize]
        public async Task<IActionResult> CheckOut(int redirect = 0)
        {

            if (redirect > 0)
            {
                await _cartRepository.MergeCarts();
                return RedirectToAction("GetUserCart");
            }
            else
            {
                return RedirectToAction("CheckOut", "UserOrder");
            }
        }
    }
}
