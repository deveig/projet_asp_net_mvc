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
                }
            };
            return ingredients;
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfIngredients()
        {
            // Arrange
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.GetAllIngredients())
                .ReturnsAsync(GetTestIngredients())
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            List<Ingredient> ingredients = GetTestIngredients();
            Ingredient ingredient = ingredients[0];

            // Act
            var result = await controller.Index();

            // Assert
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
            // Arrange
            var mockRepository = new Mock<IIngredientRepository>();
            mockRepository.Setup(repository => repository.AddIngredient(It.IsAny<Ingredient>()))
                .Verifiable();
            var controller = new RecipeController(mockRepository.Object);
            var ingredient = new Ingredient()
            {
                Id = 1,
                IngredientName = "salad",
                Quantity = 1,
                Unit = "piece"
            };

            // Act
            var result = await controller.Create(ingredient);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepository.Verify();
        }

        [Fact]
        public async Task Create_AddsAnInValidIngredient_AndReturnsErrorMessage()
        {
            // Arrange
            var mockRepository = new Mock<IIngredientRepository>();
            var controller = new RecipeController(mockRepository.Object);
            var ingredient = new Ingredient()
            {
                Id = 1,
                IngredientName = "5oil",
                Quantity = 5,
                Unit = "cl"
            };

            // Act
            var result = await controller.Create(ingredient);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var errorMessage = Assert.IsAssignableFrom<string>(
                viewResult.ViewData["ErrorMessage"]);
            Assert.Equal("Name and metric are words and quantity is a number.", errorMessage);
            mockRepository.Verify();
        }
    }
}