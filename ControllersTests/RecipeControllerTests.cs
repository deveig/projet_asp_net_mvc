using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MvcIngredient.Controllers;
using MvcIngredient.Models;
using MvcIngredient.Interfaces;

namespace MvcIngredient.ControllersTests
{
    public class RecipeControllerTests
    {
        private List<Ingredient> GetTestIngredients()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient
                {
                    Id = 1,
                    IngredientName = "salad",
                    Quantity = 1,
                    Unit = "piece"
                },
                new Ingredient
                {
                    Id = 2,
                    IngredientName = "oil",
                    Quantity = 5,
                    Unit = "cl"
                }
            };
            return ingredients;
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfIngredients()
        {
            // Arrange.
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetAllIngredients())
                .ReturnsAsync(GetTestIngredients())
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            List<Ingredient> ingredients = GetTestIngredients();
            Ingredient ingredient = ingredients[0];

            // Act.
            var result = await controller.Index();

            // Assert.
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Ingredient>>(
                viewResult.ViewData.Model);
            Assert.Equal(ingredient.IngredientName, model[0].IngredientName);
            Assert.Equal(ingredient.Quantity, model[0].Quantity);
            Assert.Equal(ingredient.Unit, model[0].Unit);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Create_AddsAValidIngredient_AndRedirectToActionOfIndex()
        {
            // Arrange.
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.AddIngredient(It.IsAny<Ingredient>()))
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            var ingredient = new Ingredient
            {
                Id = 1,
                IngredientName = "salad",
                Quantity = 1,
                Unit = "piece"
            };

            // Act.
            var result = await controller.Create(ingredient);

            // Assert.
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Create_AddsAnInvalidIngredient_AndReturnsErrorMessage()
        {
            // Arrange.
            var mockRepository = new Mock<IIngredientRepository>();
            var controller = new RecipeController(mockRepository.Object);
            var ingredient = new Ingredient
            {
                Id = 1,
                IngredientName = "5oil",
                Quantity = 5,
                Unit = "cl"
            };

            // Act.
            var result = await controller.Create(ingredient);

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
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetAllIngredients())
                .ReturnsAsync(GetTestIngredients())
                .Verifiable();
            mockRepository.Setup(repository => repository.DeleteIngredient(It.IsAny<Ingredient>()))
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            var value = new Dictionary<string, bool>();
            value.Add("delete", true);

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
            List<Ingredient> ingredients = new List<Ingredient>();
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetAllIngredients())
                .ReturnsAsync(ingredients)
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            var value = new Dictionary<string, bool>();
            value.Add("delete", true);

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