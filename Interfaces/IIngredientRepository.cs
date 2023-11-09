using MvcIngredient.Models;

namespace MvcIngredient.Interfaces
{
    public interface IIngredientRepository
    {
        Task<List<Ingredient>> GetAllIngredients();

        void AddIngredient(Ingredient ingredient);

        void DeleteIngredient(Ingredient lastIngredient);
    }
}