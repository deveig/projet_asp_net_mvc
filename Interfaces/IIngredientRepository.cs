using MvcIngredient.Models;

namespace MvcIngredient.Interfaces
{
    public interface IIngredientRepository
    {
        Task<List<Ingredient>> GetAllIngredients(int userId);

        void AddIngredient(Ingredient ingredient);

        void DeleteIngredient(Ingredient lastIngredient);

        void AddUser(User user);

        Task<User?> GetUserData(string userPassword);

        Task<User?> GetUser(int? userId);
    }
}