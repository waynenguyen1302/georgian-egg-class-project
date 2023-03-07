using GeorgianEgg.Data;
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
            // TODO: Handle invalid Id

            var category = _context.Categories.Find(Id);

            ViewData["CategoryName"] = category.Name;

            var products = _context.Products
                .Where(p => p.CategoryId == Id)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            return View(products);
        }
    }
}
