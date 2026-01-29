using System.Text.Json.Serialization;

namespace MenuShopper.Models;

public class Meal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsDairy { get; set; }
    public bool IsFavourite { get; set; }
    public List<string> Ingredients { get; set; } = [];

    [JsonIgnore]
    public string IngredientCount => Ingredients.Count == 0 ? "None" : $"{Ingredients.Count} item{(Ingredients.Count != 1 ? "s" : "")}";
}
