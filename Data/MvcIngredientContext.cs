using Microsoft.EntityFrameworkCore;

namespace MvcIngredient.Data
{
    public class MvcIngredientContext : DbContext
    {
        public MvcIngredientContext(DbContextOptions<MvcIngredientContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Ingredient> Ingredient { get; set; } = default!;
    }
}
