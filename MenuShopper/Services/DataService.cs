using System.Text.Json;
using GitHub.Copilot.SDK;
using MenuShopper.Models;

namespace MenuShopper.Services;

public class DataService : IAsyncDisposable
{
    private const string MealsFileName = "meals.json";
    private const string CategoriesFileName = "categories.json";
    private const string MenusFolderName = "Menus";

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly SemaphoreSlim _menuSaveLock = new(1, 1);
    private readonly CopilotClient _copilotClient = new();

    private List<Meal> _meals = [];
    private List<Menu> _menus = [];
    private List<string> _categories = [];

    private string BaseDataPath => Path.Combine(AppContext.BaseDirectory, "Data");
    private string MealsFilePath => Path.Combine(BaseDataPath, MealsFileName);
    private string MenusFolderPath => Path.Combine(BaseDataPath, MenusFolderName);
    private string CategoriesFilePath => Path.Combine(BaseDataPath, CategoriesFileName);

    public async Task<List<Meal>> LoadMealsAsync()
    {
        try
        {
            if (!File.Exists(MealsFilePath))
            {
                _meals = [];
                return _meals;
            }

            var json = await File.ReadAllTextAsync(MealsFilePath);
            _meals = JsonSerializer.Deserialize<List<Meal>>(json) ?? [];
        }
        catch
        {
            _meals = [];
        }

        return _meals;
    }

    public List<Meal> GetMeals() => _meals;

    public Meal? FindMealById(Guid id) => _meals.FirstOrDefault(m => m.Id == id);

    public async Task SaveMealsAsync()
    {
        Directory.CreateDirectory(BaseDataPath);
        var json = JsonSerializer.Serialize(_meals, _jsonOptions);
        await File.WriteAllTextAsync(MealsFilePath, json);
    }

    public async Task AddMealAsync(Meal meal)
    {
        _meals.Add(meal);
        await SaveMealsAsync();
    }

    public async Task UpdateMealAsync(Meal meal)
    {
        var existing = _meals.FirstOrDefault(m => m.Id == meal.Id);
        if (existing == null)
            return;

        existing.Name = meal.Name;
        existing.Category = meal.Category;
        existing.IsDairy = meal.IsDairy;
        existing.IsFavourite = meal.IsFavourite;
        existing.Ingredients = meal.Ingredients;
        await SaveMealsAsync();
    }

    public async Task DeleteMealAsync(Meal meal)
    {
        _meals.Remove(meal);
        await SaveMealsAsync();
    }

    public async Task<List<string>> LoadCategoriesAsync()
    {
        if (_categories.Count > 0)
            return _categories;

        try
        {
            if (!File.Exists(CategoriesFilePath))
            {
                _categories = [];
                return _categories;
            }

            var json = await File.ReadAllTextAsync(CategoriesFilePath);
            _categories = JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            _categories = [];
        }

        return _categories;
    }

    private void EnsureMenusFolder()
    {
        Directory.CreateDirectory(MenusFolderPath);
    }

    public async Task<List<Menu>> LoadMenusAsync()
    {
        _menus = [];
        EnsureMenusFolder();

        foreach (var file in Directory.GetFiles(MenusFolderPath, "menu_*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var menu = JsonSerializer.Deserialize<Menu>(json);
                if (menu != null)
                    _menus.Add(menu);
            }
            catch
            {
                // ignore invalid menus
            }
        }

        _menus = _menus.OrderByDescending(m => m.Date).ToList();
        return _menus;
    }

    public List<Menu> GetMenus() => _menus;

    public Menu? FindMenuById(Guid id) => _menus.FirstOrDefault(m => m.Id == id);

    public async Task SaveMenuAsync(Menu menu)
    {
        EnsureMenusFolder();
        await _menuSaveLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(menu, _jsonOptions);
            var path = Path.Combine(MenusFolderPath, menu.FileName);
            await File.WriteAllTextAsync(path, json);

            var existing = _menus.FirstOrDefault(m => m.Id == menu.Id);
            if (existing != null)
                _menus.Remove(existing);
            _menus.Add(menu);
            _menus = _menus.OrderByDescending(m => m.Date).ToList();
        }
        finally
        {
            _menuSaveLock.Release();
        }
    }

    public async Task DeleteMenuAsync(Menu menu)
    {
        EnsureMenusFolder();
        var path = Path.Combine(MenusFolderPath, menu.FileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Menu file not found at {path}.", path);

        File.Delete(path);
        _menus.Remove(menu);
        await Task.CompletedTask;
    }

    public List<ShoppingListItem> GenerateShoppingList(Menu menu)
    {
        var items = new Dictionary<string, ShoppingListItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var selection in menu.Meals)
        {
            var meal = FindMealById(selection.MealId);
            if (meal == null)
                continue;

            foreach (var ingredient in meal.Ingredients)
            {
                if (items.TryGetValue(ingredient, out var existing))
                {
                    if (!existing.MealNames.Contains(meal.Name))
                        existing.MealNames.Add(meal.Name);
                }
                else
                {
                    items[ingredient] = new ShoppingListItem
                    {
                        Ingredient = ingredient,
                        MealNames = [meal.Name],
                        IsBought = false,
                        IsCustomItem = false,
                    };
                }
            }
        }

        foreach (var existingItem in menu.ShoppingItems)
        {
            if (existingItem.IsCustomItem)
            {
                items[existingItem.Ingredient] = existingItem;
            }
            else if (items.TryGetValue(existingItem.Ingredient, out var item))
            {
                item.IsBought = existingItem.IsBought;
                item.Id = existingItem.Id;
            }
        }

        return items
            .Values.OrderBy(i => i.IsBought)
            .ThenBy(i => i.MealNames.FirstOrDefault() ?? "zzz")
            .ThenBy(i => i.Ingredient)
            .ToList();
    }

    public async Task<string> SendCopilotPromptAsync(
        string prompt,
        string model,
        string mode,
        TimeSpan timeout,
        CancellationToken cancellationToken
    )
    {
        await using var session = await _copilotClient.CreateSessionAsync(
            new SessionConfig { Model = model }
        );
        var response = await session.SendAndWaitAsync(
            new MessageOptions { Prompt = prompt, Mode = mode },
            timeout,
            cancellationToken
        );
        return response?.Data?.Content?.Trim() ?? string.Empty;
    }

    public string GetDataFolderPath() => BaseDataPath;

    public async ValueTask DisposeAsync()
    {
        _menuSaveLock.Dispose();
        await _copilotClient.DisposeAsync();
    }
}
