using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Moq;
using MvcIngredient.Controllers;
using MvcIngredient.Interfaces;
using MvcIngredient.Models;
using MvcIngredient.Models.ViewModels;
using System.Security.Cryptography;
using Xunit;
using System.Text;

namespace MvcIngredient.ControllersTests
{
    public class RecipeControllerTests
    {
        private static string HashPassword(string password)
        {
            // Generate a 128-bit salt using a sequence of
            // cryptographically strong random bytes.
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            
            return hashed;
        }
        private User GetTestUser()
        {
            string username = "henry";
            string password = HashPassword(username);
            var user = new User
            {
                UserId = 1,
                UserName = username,
                UserPassword = password
            };
            return user;
        }

        private List<Ingredient> GetTestIngredients()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient
                {
                    IngredientId = 1,
                    IngredientName = "salad",
                    Quantity = 1,
                    Unit = "piece",
                    UserId = GetTestUser().UserId
                },
                new Ingredient
                {
                    IngredientId = 2,
                    IngredientName = "oil",
                    Quantity = 5,
                    Unit = "cl",
                    UserId = GetTestUser().UserId
                }
            };
            return ingredients;
        }

        [Fact]
        public async Task CreateUser_AddsAValidUser_AndRedirectToActionOfIndex()
        {
            // Arrange.
            string username = "eva";
            var user = new User
            {
                UserName = username
            };
            var newUser = new User
            {
                UserName = username,
                UserPassword = HashPassword(username)
            };
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.AddUser(It.IsAny<User>()))
                .Verifiable();
            mockRepository.Setup(repository => repository.GetUserData(It.IsAny<String>()))
                .ReturnsAsync(newUser)
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);

            // Act.
            var result = await controller.CreateUser(user);

            // Assert.
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfIngredients()
        {
           // Arrange.
            User user = GetTestUser();
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Id", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = BitConverter.GetBytes(user.UserId);
                        return true;
                    });
            sessionMock.Setup(s => s.TryGetValue("Name", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = Encoding.UTF8.GetBytes(user.UserName!);
                        return true;
                    });
            var context = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };

            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetUser(It.IsAny<int>()))
                .ReturnsAsync(user)
                .Verifiable();
            List<Ingredient> ingredients = GetTestIngredients();
            mockRepository.Setup(repository => repository.GetAllIngredients(It.IsAny<int>()))
               .ReturnsAsync(ingredients)
               .Verifiable();
           
            var controller = new RecipeController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = context
                };

           Ingredient ingredient = ingredients[0];

           // Act.
           var result = await controller.Index();

           // Assert.
           var viewResult = Assert.IsType<ViewResult>(result);
           var username = Assert.IsAssignableFrom<string>(
                viewResult.ViewData["UserName"]);
           var viewModelIngredient = Assert.IsAssignableFrom<UserIngredientsViewModel>(
               viewResult.ViewData.Model);
           Assert.Equal(user.UserName, username);
           Assert.Equal(ingredient.IngredientName, viewModelIngredient.Ingredients[0].IngredientName);
           Assert.Equal(ingredient.Quantity, viewModelIngredient.Ingredients[0].Quantity);
           Assert.Equal(ingredient.Unit, viewModelIngredient.Ingredients[0].Unit);
           mockRepository.Verify();
        }

         [Fact]// verify
         public async Task CreateIngredient_AddsAValidIngredient_AndRedirectToActionOfIndex()
         {
             // Arrange.
            User user = GetTestUser();
            var ingredient = new Ingredient
             {
                 IngredientId = 1,
                 IngredientName = "salad",
                 Quantity = 1,
                 Unit = "piece",
                 User = user
             };
            
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Id", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = BitConverter.GetBytes(user.UserId);
                        return true;
                    });

            var context = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };
             var mockRepository = new Mock<IIngredientRepository>();
             mockRepository.Setup(repository => repository.GetUser(It.IsAny<int>()))
                .ReturnsAsync(user)
                .Verifiable();
             mockRepository.Setup(repository => repository.AddIngredient(It.IsAny<Ingredient>()))
                .Verifiable();
            
            var controller = new RecipeController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = context
                };
             // Act.
             var result = await controller.CreateIngredient(ingredient);

             // Assert.
             var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
             Assert.Equal("Index", redirectToActionResult.ActionName);
             mockRepository.Verify();
         }

        [Fact]
        public async Task CreateIngredient_AddsAnInvalidIngredient_AndReturnsErrorMessage()
        {
            // Arrange.
            User user = GetTestUser();
            var ingredient = new Ingredient
            {
                IngredientId = 1,
                IngredientName = "5oil",
                Quantity = 5,
                Unit = "cl",
                User = user
            };
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Id", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = BitConverter.GetBytes(user.UserId);
                        return true;
                    });

            var context = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetUser(It.IsAny<int>()))
                .ReturnsAsync(GetTestUser())
                .Verifiable();
            
            var controller = new RecipeController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = context
                };
            // Act.
            var result = await controller.CreateIngredient(ingredient);

            // Assert.
            var viewResult = Assert.IsType<ViewResult>(result);
            var errorMessage = Assert.IsAssignableFrom<string>(
                viewResult.ViewData["ErrorMessage"]);
            Assert.Equal("Name and metric are words and quantity is a number.", errorMessage);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Delete_TheLastIngredient_AndReturnsWarningMessage()
        {
            // Arrange.
            var value = new Dictionary<string, bool>();
            value.Add("delete", true);
            User user = GetTestUser();
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Id", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = BitConverter.GetBytes(user.UserId);
                        return true;
                    });
            sessionMock.Setup(s => s.TryGetValue("Name", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = Encoding.UTF8.GetBytes(user.UserName!);
                        return true;
                    });
            var context = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetUser(It.IsAny<int>()))
                .ReturnsAsync(user)
                .Verifiable();
            mockRepository.Setup(repository => repository.GetAllIngredients(It.IsAny<int>()))
                .ReturnsAsync(GetTestIngredients())
                .Verifiable();
            mockRepository.Setup(repository => repository.DeleteIngredient(It.IsAny<Ingredient>()))
                .Verifiable();
           
            var controller = new RecipeController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = context
                };
            
            // Act.
            var result = await controller.Delete(value);

            // Assert.
            var viewResult = Assert.IsType<ViewResult>(result);
            var warningMessage = Assert.IsAssignableFrom<string>(
                viewResult.ViewData["WarningMessage"]);
            Assert.Equal("Last ingredient removed.", warningMessage);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Delete_WhenNoIngredientToDelete_AndReturnsWarningMessage()
        {
            // Arrange.
            var value = new Dictionary<string, bool>();
            value.Add("delete", true);
            User user = GetTestUser();
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Id", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = BitConverter.GetBytes(user.UserId);
                        return true;
                    });
            sessionMock.Setup(s => s.TryGetValue("Name", out It.Ref<byte[]>.IsAny))
                    .Returns((string _, out byte[] value) =>
                    {
                        value = Encoding.UTF8.GetBytes(user.UserName!);
                        return true;
                    });
            var context = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetUser(It.IsAny<int>()))
                .ReturnsAsync(user)
                .Verifiable();
            List<Ingredient> ingredients = new List<Ingredient>();
            mockRepository.Setup(repository => repository.GetAllIngredients(It.IsAny<int>()))
                .ReturnsAsync(ingredients)
                .Verifiable();
            
            var controller = new RecipeController(mockRepository.Object);
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = context
                };

            // Act.
            var result = await controller.Delete(value);

            // Assert.
            var viewResult = Assert.IsType<ViewResult>(result);
            var warningMessage = Assert.IsAssignableFrom<string>(
                viewResult.ViewData["WarningMessage"]);
            Assert.Equal("No ingredient to remove.", warningMessage);
            mockRepository.Verify();
        }
    }
}