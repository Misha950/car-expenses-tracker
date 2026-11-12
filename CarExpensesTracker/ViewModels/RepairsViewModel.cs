using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CarExpensesTracker.Data;
using CarExpensesTracker.Models;
using CarExpensesTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarExpensesTracker.ViewModels;

// Один рядок списку: запис + кнопки Редагувати/Видалити з підтвердженням
public partial class RepairRow : ObservableObject
{
    private readonly RepairsViewModel _owner;

    public ServiceRecord Record { get; }

    public bool IsRepair => Record.Type == "Ремонт";

    public bool IsService => !IsRepair;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotConfirming))]
    private bool _confirmingDelete;

    public bool NotConfirming => !ConfirmingDelete;

    public RepairRow(ServiceRecord record, RepairsViewModel owner)
    {
        Record = record;
        _owner = owner;
    }

    [RelayCommand]
    private void Edit() => _owner.StartEdit(this);

    [RelayCommand]
    private void AskDelete() => ConfirmingDelete = true;

    [RelayCommand]
    private void CancelDelete() => ConfirmingDelete = false;

    [RelayCommand]
    private void ConfirmDelete() => _owner.Delete(this);
}

public partial class RepairsViewModel : ViewModelBase
{
    private RepairRow? _editingRow;

    public ObservableCollection<RepairRow> Rows { get; } = new();

    public string[] Types { get; } = { "Ремонт", "ТО" };

    [ObservableProperty]
    private DateTime? _newDate = DateTime.Today;

    [ObservableProperty]
    private string _newType = "ТО";

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private string _newCost = string.Empty;

    [ObservableProperty]
    private string _newMileage = string.Empty;

    [ObservableProperty]
    private DateTime? _newNextServiceDate;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    private bool _isEditing;

    public string SaveButtonText => IsEditing ? "Зберегти" : "Додати";

    public RepairsViewModel()
    {
        LoadRecords();
    }

    private void LoadRecords()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        Rows.Clear();
        foreach (var record in db.ServiceRecords.OrderByDescending(r => r.Date).ToList())
        {
            Rows.Add(new RepairRow(record, this));
        }
    }

    private void ClearForm()
    {
        _editingRow = null;
        IsEditing = false;
        NewDate = DateTime.Today;
        NewType = "ТО";
        NewDescription = string.Empty;
        NewCost = string.Empty;
        NewMileage = string.Empty;
        NewNextServiceDate = null;
        ErrorMessage = string.Empty;
    }

    public void StartEdit(RepairRow row)
    {
        _editingRow = row;
        IsEditing = true;
        ErrorMessage = string.Empty;

        NewDate = row.Record.Date;
        NewType = row.Record.Type;
        NewDescription = row.Record.Description;
        NewCost = row.Record.Cost.ToString("0.##", CultureInfo.InvariantCulture);
        NewMileage = row.Record.Mileage.ToString(CultureInfo.InvariantCulture);
        NewNextServiceDate = row.Record.NextServiceDate;
    }

    public void Delete(RepairRow row)
    {
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            var record = db.ServiceRecords.Find(row.Record.Id);
            if (record is not null)
            {
                db.ServiceRecords.Remove(record);
                db.SaveChanges();
                LogService.Info($"Видалено {record.Type}: {record.Description}, {record.Cost} грн");
            }
        }

        if (_editingRow?.Record.Id == row.Record.Id)
        {
            ClearForm();
        }

        LoadRecords();
    }

    [RelayCommand]
    private void CancelEdit() => ClearForm();

    [RelayCommand]
    private void Save()
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

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();

            if (_editingRow is null)
            {
                var record = new ServiceRecord
                {
                    Date = NewDate ?? DateTime.Today,
                    Type = NewType,
                    Description = NewDescription.Trim(),
                    Cost = cost,
                    Mileage = mileage,
                    NextServiceDate = NewNextServiceDate
                };

                db.ServiceRecords.Add(record);
                db.SaveChanges();
                LogService.Info($"Додано {record.Type}: {record.Description}, {record.Cost} грн, пробіг {record.Mileage}");
            }
            else
            {
                var record = db.ServiceRecords.Find(_editingRow.Record.Id);
                if (record is not null)
                {
                    record.Date = NewDate ?? DateTime.Today;
                    record.Type = NewType;
                    record.Description = NewDescription.Trim();
                    record.Cost = cost;
                    record.Mileage = mileage;
                    record.NextServiceDate = NewNextServiceDate;

                    db.SaveChanges();
                    LogService.Info($"Змінено {record.Type}: {record.Description}, {record.Cost} грн, пробіг {record.Mileage}");
                }
            }
        }

        ClearForm();
        LoadRecords();
    }
}