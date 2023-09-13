using System.ComponentModel.DataAnnotations;

namespace MvcIngredient.Models;

public class Ingredient
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Name")]
    public string? IngredientName { get; set; }

    [Required]
    [RangeAttribute(1, 1000000)]
    public int Quantity { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Metric")]
    public string? Unit { get; set; }
}