using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Primitives;
using static System.Net.WebRequestMethods;

namespace AspNetOnlineBookstore.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepository;

        public UserOrderController(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }

        public async Task<IActionResult> CheckOut(int redirect = 0)
        {
            var orderToPlace = await _userOrderRepository.ReviewOrder();
            if(orderToPlace != null)
            {
                return View(orderToPlace);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        public async Task<IActionResult> OrderPlaced()
        {
            string referer = Request.Headers["Referer"].ToString();
            if (referer == "http://ec2-3-135-198-180.us-east-2.compute.amazonaws.com/UserOrder/CheckOut")
            {
                var orderPlaced = await _userOrderRepository.PlaceOrder();
                return View(orderPlaced);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepository.GetUserOrders();
            return View(orders);
        }
    }
}
