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
public partial class FuelRow : ObservableObject
{
    private readonly FuelViewModel _owner;

    public FuelRecord Record { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotConfirming))]
    private bool _confirmingDelete;

    public bool NotConfirming => !ConfirmingDelete;

    public FuelRow(FuelRecord record, FuelViewModel owner)
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

public partial class FuelViewModel : ViewModelBase
{
    private FuelRow? _editingRow;

    public ObservableCollection<FuelRow> Rows { get; } = new();

    [ObservableProperty]
    private DateTime? _newDate = DateTime.Today;

    [ObservableProperty]
    private string _newLiters = string.Empty;

    [ObservableProperty]
    private string _newPricePerLiter = string.Empty;

    [ObservableProperty]
    private string _newMileage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasRows;

    public bool IsEmpty => !HasRows;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    private bool _isEditing;

    public string SaveButtonText => IsEditing ? "Зберегти" : "Додати";

    public FuelViewModel()
    {
        LoadRecords();
    }

    private void LoadRecords()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        Rows.Clear();
        foreach (var record in db.FuelRecords.OrderByDescending(r => r.Date).ThenByDescending(r => r.Id).ToList())
        {
            Rows.Add(new FuelRow(record, this));
        }

        HasRows = Rows.Count > 0;
    }

    private void ClearForm()
    {
        _editingRow = null;
        IsEditing = false;
        NewDate = DateTime.Today;
        NewLiters = string.Empty;
        NewPricePerLiter = string.Empty;
        NewMileage = string.Empty;
        ErrorMessage = string.Empty;
    }

    public void StartEdit(FuelRow row)
    {
        _editingRow = row;
        IsEditing = true;
        ErrorMessage = string.Empty;

        NewDate = row.Record.Date;
        NewLiters = row.Record.Liters.ToString("0.##", CultureInfo.InvariantCulture);
        NewPricePerLiter = row.Record.PricePerLiter.ToString("0.##", CultureInfo.InvariantCulture);
        NewMileage = row.Record.Mileage.ToString(CultureInfo.InvariantCulture);
    }

    public void Delete(FuelRow row)
    {
        try
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                var record = db.FuelRecords.Find(row.Record.Id);
                if (record is not null)
                {
                    db.FuelRecords.Remove(record);
                    db.SaveChanges();
                    LogService.Info($"Видалено заправку: {record.Liters} л, {record.TotalCost} грн");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не вдалося видалити запис";
            LogService.Info($"Помилка видалення заправки: {ex.Message}");
            return;
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

        if (!TryParseDecimal(NewLiters, out var liters) || liters <= 0)
        {
            ErrorMessage = "Літри мають бути додатним числом";
            return;
        }

        if (!TryParseDecimal(NewPricePerLiter, out var price) || price <= 0)
        {
            ErrorMessage = "Ціна за літр має бути додатним числом";
            return;
        }

        if (!int.TryParse(NewMileage.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var mileage) || mileage <= 0)
        {
            ErrorMessage = "Пробіг має бути цілим додатним числом";
            return;
        }

        try
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                if (_editingRow is null)
                {
                    var record = new FuelRecord
                    {
                        Date = NewDate ?? DateTime.Today,
                        Liters = liters,
                        PricePerLiter = price,
                        Mileage = mileage
                    };

                    db.FuelRecords.Add(record);
                    db.SaveChanges();
                    LogService.Info($"Додано заправку: {record.Liters} л, {record.TotalCost} грн");
                }
                else
                {
                    var record = db.FuelRecords.Find(_editingRow.Record.Id);
                    if (record is not null)
                    {
                        record.Date = NewDate ?? DateTime.Today;
                        record.Liters = liters;
                        record.PricePerLiter = price;
                        record.Mileage = mileage;

                        db.SaveChanges();
                        LogService.Info($"Змінено заправку: {record.Liters} л, {record.TotalCost} грн");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не вдалося зберегти запис";
            LogService.Info($"Помилка збереження заправки: {ex.Message}");
            return;
        }

        ClearForm();
        LoadRecords();
    }

    private static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
}
