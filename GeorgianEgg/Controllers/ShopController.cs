using GeorgianEgg.Data;
using GeorgianEgg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace GeorgianEgg.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ShopController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET Shop/Index
        public IActionResult Index()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        // GET Shop/Category?Id=123
        public IActionResult Category(int Id)
        {
            var category = _context.Categories.Find(Id);

            if (category == null)
            {
                return NotFound();
            }

            ViewData["CategoryName"] = category.Name;

            var products = _context.Products
                .Where(p => p.CategoryId == Id)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            return View(products);
        }

        // GET Shop/Cart
        public IActionResult Cart()
        {
            var customerId = GetCustomerId();

            var cartLines = _context.CartLines
                .Where(cl => cl.CustomerId == customerId)
                .Include(cl => cl.Product)
                .OrderByDescending(cl => cl.Id)
                .ToList();

            ViewData["TotalPrice"] = cartLines.Sum(cl => cl.Price).ToString("C");

            /*
            decimal totalPrice = 0;
            for (int i = 0; i < cartLines.Count(); i++)
            {
                CartLine cartLine = cartLines[i];
                totalPrice += cartLine.Price;
            }
            ViewData["TotalPrice"] = totalPrice.ToString("C");
            */

            return View(cartLines);
        }

        // GET Shop/Checkout
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var customerId = GetCustomerId();

            var totalPrice = _context.CartLines
                .Where(cl => cl.CustomerId == customerId)
                .Sum(cl => cl.Price)
                .ToString("C");

            ViewData["Total"] = totalPrice;

            return View();
        }

        // POST Shop/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            [Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order Order
        )
        {
            // Build Order
            Order.OrderDate = DateTime.UtcNow;
            Order.CustomerId = GetCustomerId();
            Order.InProgress = true;

            // Remove old InProgress orders for the customer
            var oldOrders = await _context.Orders
                .Where(o => o.InProgress && o.CustomerId == Order.CustomerId)
                .ToListAsync();

            foreach (var oldOrder in oldOrders)
            {
                _context.Orders.Remove(oldOrder);
            }

            // Get cart lines
            var cartLines = await _context.CartLines
                .Where(cl => cl.CustomerId == Order.CustomerId)
                .ToListAsync();

            Order.Total = cartLines.Sum(cl => cl.Price);

            // Create order
            var CreatedOrder = await _context.Orders.AddAsync(Order);
            await _context.SaveChangesAsync();

            foreach (var cartLine in cartLines)
            {
                // Create OrderLine
                await _context.OrderLines.AddAsync(new OrderLine()
                {
                    Quantity = cartLine.Quantity,
                    Price = cartLine.Price,
                    ProductId = cartLine.ProductId,

                    OrderId = CreatedOrder.Entity.Id,
                });
            }

            await _context.SaveChangesAsync();

            return Redirect("Payment");
        }

        // GET Shop/Payment
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Payment()
        {
            var customerId = GetCustomerId();

            var order = await GetCurrentOrderAsync(customerId);

            if (order == null)
            {
                return NotFound();
            }

            ViewData["Total"] = order.Total;

            ViewData["PublishableKey"] = _config["Payments:Stripe:PublishableKey"];

            return View();
        }

        // POST Shop/Payment
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Payment(String StripeToken)
        {
            var customerId = GetCustomerId();
            var order = await GetCurrentOrderAsync(customerId);

            // can't access payment process unless there is an ongoing order
            if (order == null)
            {
                return NotFound();
            }

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long) (order.Total * 100),
                            Currency = "CAD",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Store Purchase",
                            },
                        },
                        Quantity = 1,
                    },
                },
                PaymentMethodTypes = new List<String>
                {
                    "card",
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/Cart",
            };

            var service = new SessionService();
            var session = service.Create(options);

            Response.Headers.Add("Status", "303");
            Response.Headers.Add("Location", session.Url);

            return Json(new { id = session.Id });
        }

        // GET /Shop/SaveOrder
        [Authorize]
        public async Task<IActionResult> SaveOrder()
        {
            var customerId = GetCustomerId();
            var order = await GetCurrentOrderAsync(customerId);

            if (order == null)
            {
                return NotFound();
            }

            order.InProgress = false;

            var cartLines = await _context.CartLines
                .Where(cl => cl.CustomerId == customerId)
                .ToListAsync();

            foreach (var cartLine in cartLines)
            {
                _context.CartLines.Remove(cartLine);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { @id = order.Id });
        }

        // POST Shop/AddToCart
        [HttpPost]
        public IActionResult AddToCart([FromForm] int ProductId, [FromForm] int Quantity)
        {
            if (Quantity <= 0)
            {
                return BadRequest();
            }

            var product = _context.Products.Find(ProductId);
            if (product == null)
            {
                return BadRequest(); // HTTP Status code 400
            }

            var price = product.Price * Quantity;

            var customerId = GetCustomerId();

            var cartLine = _context.CartLines
                .Where(cl => cl.ProductId == ProductId && cl.CustomerId == customerId)
                .FirstOrDefault();

            if (cartLine != null)
            {
                cartLine.Quantity += Quantity;
                cartLine.Price += price;

                _context.CartLines.Update(cartLine);
                _context.SaveChanges();
            }
            else
            {
                cartLine = new CartLine()
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    CustomerId = customerId,
                };

                _context.CartLines.Add(cartLine);
                _context.SaveChanges();
            }

            return Redirect("Cart");
        }

        // POST Shop/UpdateCart
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int CartLineId, [FromForm] int Quantity)
        {
            if (Quantity <= 0)
            {
                return BadRequest();
            }

            var cartLine = _context.CartLines.Find(CartLineId);

            if (cartLine == null)
            {
                return BadRequest();
            }

            var product = _context.Products.Find(cartLine.ProductId);
            var discount = cartLine.Price - (product.Price * cartLine.Quantity);

            cartLine.Quantity = Quantity;
            cartLine.Price = Math.Max((product.Price * Quantity) - discount, 0);

            _context.CartLines.Update(cartLine);
            _context.SaveChanges();

            return Redirect("Cart");
        }

        // POST Shop/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart([FromForm] int CartLineId)
        {
            var cartLine = _context.CartLines.Find(CartLineId);

            if (cartLine == null)
            {
                return BadRequest();
            }

            _context.CartLines.Remove(cartLine);
            _context.SaveChanges();

            return Redirect("Cart");
        }

        /*private String GetCustomerId()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");

            if (String.IsNullOrWhiteSpace(customerId))
            {
                // '??' is called 'null-coalescing' operator
                var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

                if (isLoggedIn)
                {
                    // 1.a If user is logged in, use their email
                    customerId = User.Identity.Name;
                }
                else
                {
                    // 1.b Else generate a unique ID

                    // UUID or GUID
                    // UUID = Universally Unique ID
                    // GUID = Globally Unique Id
                    // C# uses "GUID"

                    customerId = Guid.NewGuid().ToString();
                }

                HttpContext.Session.SetString("CustomerId", customerId);
            }

            return customerId;
        }*/

        private async Task<Order?> GetCurrentOrderAsync(String customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.InProgress && o.CustomerId == customerId)
                .ToListAsync();

            if (orders.Count() == 0)
            {
                return null;
            }

            return orders[0];
        }
        private String GetCustomerId()
        {
            var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

            if (isLoggedIn)
            {
                return User.Identity.Name;
            }
            //remember to use session in Program.cs

            var customerId = HttpContext.Session.GetString("CustomerId");

            if (String.IsNullOrWhiteSpace(customerId))
            {
                //generate new customerID
                // '??' is called 'null-coalescing' operator
                //var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

                if (User?.Identity?.IsAuthenticated ?? false)
                {
                    //1.a if user is logged in, use their email
                    customerId = User.Identity.Name;
                }
                else
                {
                    //1.b else generate a unique ID
                    //UUID or GUID: Universal Unique ID or Globally Unique ID
                    //C#: GUID
                    customerId = Guid.NewGuid().ToString();
                }


                //store the new customerID
                HttpContext.Session.SetString("CustomerId", customerId);
            }

            return customerId;
        }

    }
}


