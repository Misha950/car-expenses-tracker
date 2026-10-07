Структура рішення

car-expenses-tracker/
├── car-expenses-tracker.sln     — файл рішення .NET
├── README.md                    — опис проєкту та інструкція запуску
├── docs/                        — вся документація проєкту
├── interface/                   — макети інтерфейсу (PDF)
└── CarExpensesTracker/          — сам застосунок
    ├── Program.cs               — точка входу
    ├── App.axaml(.cs)           — налаштування застосунку, створення бази
    ├── ViewLocator.cs           — підбирає екран за назвою ViewModel
    ├── Models/                  — класи даних: FuelRecord, ServiceRecord, OtherExpense
    ├── Data/                    — AppDbContext, робота з SQLite через EF Core
    ├── ViewModels/              — логіка екранів
    ├── Views/                   — екрани (.axaml)
    ├── Assets/                  — ресурси (іконки)
    └── CarExpensesTracker.csproj — залежності проєкту

Залежності (NuGet)

| Пакет | Навіщо |
|---|---|
| Avalonia 12.1.2 | кросплатформний інтерфейс |
| CommunityToolkit.Mvvm | спрощує написання MVVM |
| Microsoft.EntityFrameworkCore.Sqlite | робота з базою SQLite |

Планується: папка Services/ (логування, розрахунки), LiveCharts2 для графіків, тестовий проєкт xUnit.