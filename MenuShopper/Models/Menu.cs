using System.Text.Json.Serialization;

namespace MenuShopper.Models;

public class Menu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Today;
    public long CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public bool IsLocked { get; set; }
    public List<MenuMealSelection> Meals { get; set; } = [];
    [JsonPropertyName("MealIds")]
    public List<Guid> MealIdsLegacy { get; set; } = [];
    public List<ShoppingListItem> ShoppingItems { get; set; } = [];

    [JsonIgnore]
    public string FileName => $"menu_{CreatedTimestamp}.json";

    [JsonIgnore]
    public string DisplayDate => Date.ToString("dddd, d MMMM yyyy");

    [JsonIgnore]
    public string ShortDate => Date.ToString("dd/MM/yyyy");

    [JsonIgnore]
    public List<Guid> MealIds
    {
        get
        {
            if (Meals.Count > 0)
                return Meals.Select(m => m.MealId).ToList();

            return MealIdsLegacy;
        }
    }

    [JsonIgnore]
    public string MealCountText => $"{MealIds.Count} meal{(MealIds.Count == 1 ? string.Empty : "s")}";

    [JsonIgnore]
    public string ItemCountText => $"{ShoppingItems.Count} item{(ShoppingItems.Count == 1 ? string.Empty : "s")}";

    [JsonIgnore]
    public string RemainingCountText
    {
        get
        {
            var remaining = ShoppingItems.Count(i => !i.IsBought);
            return $"{remaining} left";
        }
    }
}
