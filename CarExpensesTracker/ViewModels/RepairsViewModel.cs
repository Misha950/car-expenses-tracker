using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CarExpensesTracker.Data;
using CarExpensesTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarExpensesTracker.Services;

namespace CarExpensesTracker.ViewModels;

public partial class RepairsViewModel : ViewModelBase
{
    public ObservableCollection<ServiceRecord> Records { get; } = new();

    public string[] Types { get; } = { "Ремонт", "ТО" };

    [ObservableProperty]
    public partial DateTime? NewDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string NewType { get; set; } = "ТО";

    [ObservableProperty]
    public partial string NewDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewCost { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMileage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public RepairsViewModel()
    {
        LoadRecords();
    }

    private void LoadRecords()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        Records.Clear();
        foreach (var record in db.ServiceRecords.OrderByDescending(r => r.Date).ToList())
        {
            Records.Add(record);
        }
    }

    [RelayCommand]
    private void Add()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NewDescription))
        {
            ErrorMessage = "Введіть опис виконаних робіт";
            return;
        }

        var costText = NewCost.Replace(',', '.');
        if (!decimal.TryParse(costText, NumberStyles.Number, CultureInfo.InvariantCulture, out var cost) || cost <= 0)
        {
            ErrorMessage = "Вартість має бути додатним числом";
            return;
        }

        if (!int.TryParse(NewMileage, out var mileage) || mileage < 0)
        {
            ErrorMessage = "Пробіг має бути цілим невід'ємним числом";
            return;
        }

        var record = new ServiceRecord
        {
            Date = NewDate ?? DateTime.Today,
            Type = NewType,
            Description = NewDescription.Trim(),
            Cost = cost,
            Mileage = mileage
        };

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            db.ServiceRecords.Add(record);
            db.SaveChanges();
        }
        LogService.Info($"Додано {record.Type}: {record.Description}, {record.Cost} грн, пробіг {record.Mileage}");

        NewDescription = string.Empty;
        NewCost = string.Empty;
        NewMileage = string.Empty;

        LoadRecords();
    }
}