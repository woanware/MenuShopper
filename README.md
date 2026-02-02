# MenuShopper

MenuShopper is a local-first meal planner that turns your meal ideas into clean, actionable shopping lists. Build a menu in minutes, track what you need to buy, and keep your favorites close.

## Why it is fun to use
- **Fast flow**: pick meals, set a date, generate a list, done.
- **Clutter-free**: every list shows where items came from and what is left.
- **Personal library**: meals, categories, favorites, and dairy flags stay organized.
- **Smart extras**: an AI emoji list generator adds a playful summary for menus.

## Feature tour
**Meals**
- Create, edit, clone, and delete meals
- Add ingredients and tag meals by category
- Mark favorites and dairy-friendly meals

**Menus**
- Build a weekly menu from your meals
- Set a start date and lock the menu
- View menu details with item and meal counts

**Shopping lists**
- Auto-generated from menu ingredients
- Add custom items any time
- Toggle items as bought with clear visual feedback

**Dashboard**
- Totals for meals, favorites, dairy meals, and unique ingredients

## Screenshots
![Menu planning](menu.png)
![Meals list](meal.png)
![Edit meal](mealedit.png)
![Shopping list](lines.png)

## Getting started
```bash
cd src
dotnet restore
dotnet run
```

Then open the URL shown in the console.

## Data location
The app stores data in a local `data` folder relative to the project root. You can view the exact path in **Settings**.

## Tech stack
- ASP.NET Core (Razor Components)
- MudBlazor UI
- GitHub Copilot SDK (emoji list generation)
