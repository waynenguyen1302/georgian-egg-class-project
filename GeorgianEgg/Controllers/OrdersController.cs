using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GeorgianEgg.Data;
using GeorgianEgg.Models;
using Microsoft.AspNetCore.Authorization;

namespace GeorgianEgg.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var query = _context.Orders.AsQueryable();

            // Non-admins can only see their own orders
            if (!User.IsInRole("Administrator"))
            {
                var customerId = User.Identity.Name;

                query = query.Where(o => o.CustomerId == customerId);
            }

            var orders = await query
                .Where(o => !o.InProgress)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null || order.InProgress)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator") && order.CustomerId != User.Identity.Name)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}