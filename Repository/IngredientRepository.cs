using Microsoft.EntityFrameworkCore;
using MvcIngredient.Interfaces;
using MvcIngredient.Data;
using MvcIngredient.Models;

namespace MvcIngredient.Repository
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly MvcIngredientContext _context;

        public IngredientRepository(MvcIngredientContext context)
        {
            _context = context;
        }
        public async Task<List<Ingredient>> GetAllIngredients(int userId)
        {
            return true ?
                await _context.Ingredient.Where(ingredient => ingredient.UserId == userId).ToListAsync() : null;
        }
        
        public async void AddIngredient(Ingredient ingredient)
        {
            _context.Add(ingredient);
            await _context.SaveChangesAsync();
        }

        public async void DeleteIngredient(Ingredient lastIngredient)
        {
            _context.Remove(lastIngredient);
            await _context.SaveChangesAsync();
        }
        public async void AddUser(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetUserData(string userPassword)
        {
            return  _context.User != null ?
                await _context.User.Where(user => user.UserPassword == userPassword).SingleAsync() : null;
        }
        public async Task<User?> GetUser(int? userId)
        {
            return _context.User != null ?
                await _context.User.FindAsync(userId) : null;
        }
    }
}