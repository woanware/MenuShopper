namespace MenuShopper.Models;

public class UserSettings
{
    public const int DefaultMealSuggestionsLookbackWeeks = 3;
    public const int MinMealSuggestionsLookbackWeeks = 1;
    public const int MaxMealSuggestionsLookbackWeeks = 8;

    public bool HasSeenAutoStartPrompt { get; set; }
    public List<string> DefaultMealNames { get; set; } = ["Oven", "Easy"];
    public int MealSuggestionsLookbackWeeks { get; set; } = DefaultMealSuggestionsLookbackWeeks;
}

