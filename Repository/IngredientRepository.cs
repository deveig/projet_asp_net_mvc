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

        public async Task<List<Ingredient>> GetAllIngredients()
        {
            return await _context.Ingredient.ToListAsync();
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
    }
}