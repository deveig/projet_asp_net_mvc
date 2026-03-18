using System.ComponentModel.DataAnnotations;

namespace MvcIngredient.Models.ViewModels

{
    public class UserIngredientsViewModel
    {
        public User? User { get; set; }

        public List<Ingredient>? Ingredients { get; set; }
    }
}