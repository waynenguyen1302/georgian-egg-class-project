using GeorgianEgg.Data;
using GeorgianEgg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeorgianEgg.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        public IActionResult Category(int Id)
        {
            // Handle invalid Id

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

        //week 9 POST /Shop/AddToCart
        [HttpPost]
        public IActionResult AddToCart([FromForm] int ProductId, int Quantity)
        {
            if(Quantity <= 0)
            {
                return BadRequest();
            }            

            var product = _context.Products.Find(ProductId);

            if (product == null)
            {
                return BadRequest();
            }

            var price = product.Price * Quantity;

            var customerId = GetCustomerId();

            var cartLine = new CartLine()
            {
                ProductId = ProductId,
                Quantity = Quantity,
                Price = price,
                CustomerId = customerId,
            };


            _context.CartLines.Add(cartLine);
            _context.SaveChanges();

            return Redirect("Cart");
        }

        private String GetCustomerId()
        {   
            //remember to use session in Program.cs
            
            var customerId = HttpContext.Session.GetString("CustomerId");

            if (String.IsNullOrEmpty(customerId))
            {
                //generate new customerID
                // '??' is called 'null-coalescing' operator
                //var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

                if(User?.Identity?.IsAuthenticated ?? false)
                {
                    //1.a if user is logged in, use their email
                    customerId = User.Identity.Name;
                }
                else
                {
                    //1.b else generate a unique ID
                    //UUID or GUID: Universal Unique ID or Globally Unique ID
                    //C#: GUID
                    customerId = 
                }



                //store the new customerID
                HttpContext.Session.SetString("CustomerId", customerId);
            }

            return customerId;
        }
    }
}
