using GeorgianEgg.Controllers;
using GeorgianEgg.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeorgianEggTests
{
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void IndexLoadsViewWithProducts()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(dbOptions);

            var controller = new ProductsController(context);

            // TODO
        }
    }
}