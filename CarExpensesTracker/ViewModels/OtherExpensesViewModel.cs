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
public partial class OtherExpenseRow : ObservableObject
{
    private readonly OtherExpensesViewModel _owner;

    public OtherExpense Record { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotConfirming))]
    private bool _confirmingDelete;

    public bool NotConfirming => !ConfirmingDelete;

    public OtherExpenseRow(OtherExpense record, OtherExpensesViewModel owner)
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

public partial class OtherExpensesViewModel : ViewModelBase
{
    private OtherExpenseRow? _editingRow;

    public ObservableCollection<OtherExpenseRow> Rows { get; } = new();

    public string[] Types { get; } = { "Штраф", "Страховка", "Паркування", "Інше" };

    [ObservableProperty]
    private DateTime? _newDate = DateTime.Today;

    [ObservableProperty]
    private string _newType = "Штраф";

    [ObservableProperty]
    private string _newAmount = string.Empty;

    [ObservableProperty]
    private string _newComment = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    private bool _isEditing;

    public string SaveButtonText => IsEditing ? "Зберегти" : "Додати";

    public OtherExpensesViewModel()
    {
        LoadRecords();
    }

    private void LoadRecords()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        Rows.Clear();
        foreach (var record in db.OtherExpenses.OrderByDescending(r => r.Date).ToList())
        {
            Rows.Add(new OtherExpenseRow(record, this));
        }
    }

    private void ClearForm()
    {
        _editingRow = null;
        IsEditing = false;
        NewDate = DateTime.Today;
        NewType = "Штраф";
        NewAmount = string.Empty;
        NewComment = string.Empty;
        ErrorMessage = string.Empty;
    }

    public void StartEdit(OtherExpenseRow row)
    {
        _editingRow = row;
        IsEditing = true;
        ErrorMessage = string.Empty;

        NewDate = row.Record.Date;
        NewType = row.Record.Type;
        NewAmount = row.Record.Amount.ToString("0.##", CultureInfo.InvariantCulture);
        NewComment = row.Record.Comment;
    }

    public void Delete(OtherExpenseRow row)
    {
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            var record = db.OtherExpenses.Find(row.Record.Id);
            if (record is not null)
            {
                db.OtherExpenses.Remove(record);
                db.SaveChanges();
                LogService.Info($"Видалено витрату {record.Type}: {record.Amount} грн");
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

        if (string.IsNullOrWhiteSpace(NewType))
        {
            ErrorMessage = "Оберіть тип витрати";
            return;
        }

        var amountText = NewAmount.Replace(',', '.');
        if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorMessage = "Сума має бути додатним числом";
            return;
        }

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();

            if (_editingRow is null)
            {
                var record = new OtherExpense
                {
                    Date = NewDate ?? DateTime.Today,
                    Type = NewType,
                    Amount = amount,
                    Comment = NewComment.Trim()
                };

                db.OtherExpenses.Add(record);
                db.SaveChanges();
                LogService.Info($"Додано витрату {record.Type}: {record.Amount} грн");
            }
            else
            {
                var record = db.OtherExpenses.Find(_editingRow.Record.Id);
                if (record is not null)
                {
                    record.Date = NewDate ?? DateTime.Today;
                    record.Type = NewType;
                    record.Amount = amount;
                    record.Comment = NewComment.Trim();

                    db.SaveChanges();
                    LogService.Info($"Змінено витрату {record.Type}: {record.Amount} грн");
                }
            }
        }

        ClearForm();
        LoadRecords();
    }
}