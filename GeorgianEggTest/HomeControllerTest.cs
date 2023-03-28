using GeorgianEgg.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GeorgianEggTests
{
    [TestClass]
    public class HomeControllerTest
    {
        private HomeController controller;
        [TestInitialize]
        public void TestInitialize()
        {
            //Arrange - setup data for the test
            controller = new HomeController();
        }
        [TestMethod]
        public void IndexIsNotNull()
        {
            TestInitialize();

            //Act - actually doing the action that we're testing
            var result = controller.Index();

            //Assert - checking if the result is what we expect
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexLoadsIndexView()
        {
            TestInitialize();

            //Act - actually doing the action that we're testing
            ViewResult result = (ViewResult)controller.Index();

            //Assert - checking if the result is what we expect
            Assert.AreEqual("Index", result.ViewName);
        }

        //TDD - Test-Driven Development

    }
}