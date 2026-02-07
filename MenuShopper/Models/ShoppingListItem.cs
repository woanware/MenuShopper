using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MenuShopper.Models;

public class ShoppingListItem : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private string _ingredient = string.Empty;
    public string Ingredient
    {
        get => _ingredient;
        set => SetProperty(ref _ingredient, value);
    }

    private List<string> _mealNames = [];
    public List<string> MealNames
    {
        get => _mealNames;
        set
        {
            if (SetProperty(ref _mealNames, value))
            {
                OnPropertyChanged(nameof(MealNamesText));
            }
        }
    }

    private bool _isBought;
    public bool IsBought
    {
        get => _isBought;
        set => SetProperty(ref _isBought, value);
    }

    private bool _isCustomItem;
    public bool IsCustomItem
    {
        get => _isCustomItem;
        set
        {
            if (SetProperty(ref _isCustomItem, value))
            {
                OnPropertyChanged(nameof(MealNamesText));
            }
        }
    }

    [JsonIgnore]
    public string MealNamesText => MealNames.Count > 0
        ? $"({string.Join(", ", MealNames)})"
        : "(Custom item)";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
