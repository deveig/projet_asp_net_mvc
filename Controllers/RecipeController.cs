using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using MvcIngredient.Interfaces;
using MvcIngredient.Models;
using MvcIngredient.Models.ViewModels;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace MvcIngredient.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IIngredientRepository _repository;

        public RecipeController(IIngredientRepository repository)
        {
            _repository = repository;
        }

        /*public const string HttpContext?.Session?.UserId = "_Id";
        public const string HttpContext?.Session?.UserName = "_Name";*/

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

        // GET: Recipe.
        public async Task<IActionResult> Index()
        {
            var ingredientsView = new List<Ingredient>();
            var userView = new User("");

            var viewModel = new UserIngredientsViewModel
            {
                User = userView,
                Ingredients = ingredientsView
            };
            if (HttpContext?.Session?.GetInt32("Id") != null)
            {
                var userId = HttpContext?.Session?.GetInt32("Id");
            
                User? user = await _repository.GetUser(userId);
                viewModel.User = user;
                if (user != null)
                {
                    ViewData["UserName"] = HttpContext?.Session?.GetString("Name");
                    List<Ingredient> ingredients = await _repository.GetAllIngredients(user!.UserId);

                    if (ingredients != null)
                    {
                        ViewData["ErrorMessage"] = "";
                        viewModel.Ingredients = ingredients;

                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "Internal Server Error.";
                    }
                } else
                {
                    ViewData["ErrorMessage"] = "";
                    ViewData["UserName"] = userView.UserName;
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "";
                ViewData["UserName"] = userView.UserName;
            }
            return View(viewModel);
        }

        // GET: Recipe/Create.
        public IActionResult CreateIngredient()
        {
            if (HttpContext?.Session?.GetInt32("Id") != null)
            {

                ViewData["UserName"] = HttpContext?.Session?.GetString("Name");
            }
            else
            {
                ViewData["UserName"] = "";
            }
                return View();
        }

        public IActionResult CreateUser()
        {
            ViewData["ErrorMessage"] = "";
            ViewData["UserName"] = "";
            return View();
        }

        // POST: Recipe/Create.
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([Bind("UserName")] User user)
        {
            var numberPattern = @"\d+";
            Regex numberRegExp = new(numberPattern);
  
            if (ModelState.IsValid)
            {
                // Verify if 'IngredientName' and 'Unit' doesn't contain a number and 'Quantity' doesn't contain a letter.
                if (!numberRegExp.IsMatch(user!.UserName!))
                {
                    string userPassword = HashPassword(user!.UserName!);
                    User newUser = new()
                    {
                        UserName = user!.UserName
                    };
                    newUser.UserPassword = userPassword;
                    _repository.AddUser(newUser);
                    User? userAdded = await _repository.GetUserData(userPassword);
                    if (userAdded != null)
                    {
                        int userId = userAdded.UserId;
                        string userName =  userAdded.UserName!;
                        HttpContext?.Session?.SetInt32("Id", userId!);
                        HttpContext?.Session?.SetString("Name", userName!);
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["ErrorMessage"] = "Enter your name!";
                    ViewData["UserName"] = "";
                }
            }
            return View();
        }

        // POST: Recipe/Create.
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIngredient([Bind(["IngredientName", "Quantity", "Unit"])] Ingredient ingredient)
        {
            
            if (HttpContext?.Session?.GetInt32("Id") != null)
            {
                var userId = HttpContext?.Session?.GetInt32("Id");
                User? user = await _repository.GetUser(userId);
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
                        if (ingredient is not null)
                        {
                            Ingredient newIngredient = new()
                            {
                                IngredientName = ingredient.IngredientName,
                                Quantity = ingredient.Quantity,
                                Unit = ingredient.Unit,
                                UserId = user!.UserId
                            };
                            _repository.AddIngredient(newIngredient);
                        }
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "Name and metric are words and quantity is a number.";
                    }
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Please, enter your name !";
            }
            return View();
        }

        // GET: Recipe/Delete.
        public IActionResult Delete()
        {
            ViewData["WarningMessage"] = "";
            if (HttpContext?.Session?.GetInt32("Id") != null)
            {

                ViewData["UserName"] = HttpContext?.Session?.GetString("Name");
            }
            else
            {
                ViewData["UserName"] = "";
            }
            return View();
        }

        // POST: Recipe/Delete.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([Bind(["delete"])] Dictionary<string, bool> value)
        {
            
            if (HttpContext?.Session?.GetInt32("Id") != null)
            {
                ViewData["UserName"] = HttpContext?.Session?.GetString("Name");
                var userId = HttpContext?.Session?.GetInt32("Id");
                User? user = await _repository.GetUser(userId);
                List<Ingredient>? ingredients = await _repository.GetAllIngredients(user!.UserId);

                if ((ingredients != null) && (value["delete"] == true))
                {
                    if (ingredients!.Count > 0)
                    {
                        Ingredient lastIngredient = ingredients[^1];
                        _repository.DeleteIngredient(lastIngredient);
                        ViewData["WarningMessage"] = "Last ingredient removed.";
                    }
                    else
                    {
                        ViewData["WarningMessage"] = "No ingredient to remove.";
                    }
                }
                else
                {
                    ViewData["WarningMessage"] = "Internal Server Error.";
                }
            }
            else
            {
                ViewData["UserName"] = "";
                ViewData["WarningMessage"] = "Please, enter your name !";
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