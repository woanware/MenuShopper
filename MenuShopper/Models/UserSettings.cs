namespace MenuShopper.Models;

public class UserSettings
{
    public bool HasSeenAutoStartPrompt { get; set; }
    public List<string> DefaultMealNames { get; set; } = ["Oven", "Easy"];
}

