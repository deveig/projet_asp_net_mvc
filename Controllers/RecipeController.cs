using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcIngredient.Models;
using MvcIngredient.Interfaces;

namespace MvcIngredient.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IIngredientRepository _repository;

        public RecipeController(IIngredientRepository repository)
        {
            _repository = repository;
        }

        // GET: Recipe 
        public async Task<IActionResult> Index()
        {

            List<Ingredient> ingredients = await _repository.GetAllIngredients();

            if (ingredients != null)
            {
                return View(ingredients);
            }
            else
            {
                return Problem("Entity set 'MvcIngredientContext.Ingredient'  is null.");
            }
        }

        // GET: Recipe/Create
        public IActionResult Create()
        {
            ViewData["ErrorMessage"] = "";
            return View();
        }

        // POST: Recipe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(new string[] { "IngredientName", "Quantity", "Unit" })] Ingredient ingredient)
        {
            var numberPattern = @"\d+";
            var letterPattern = @"\D+";
            Regex numberRegExp = new(numberPattern);
            Regex letterRegExp = new(letterPattern);
            if (ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "";
                // Verify if 'IngredientName' and 'Unit' doesn't contain a number and 'Quantity' doesn't contain a letter.
                if ((!numberRegExp.IsMatch(ingredient.IngredientName!)) && (!letterRegExp.IsMatch(ingredient.Quantity.ToString())) && (!numberRegExp.IsMatch(ingredient.Unit!)))
                {
                    _repository.AddIngredient(ingredient);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["ErrorMessage"] = "Name and metric are words and quantity is a number.";
                }
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
