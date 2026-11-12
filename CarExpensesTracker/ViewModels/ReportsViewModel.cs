using System;
using System.Linq;
using CarExpensesTracker.Data;
using CarExpensesTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace CarExpensesTracker.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial DateTime? FromDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);

    [ObservableProperty]
    public partial DateTime? ToDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ISeries[] CategorySeries { get; set; } = Array.Empty<ISeries>();

    [ObservableProperty]
    public partial ISeries[] MonthSeries { get; set; } = Array.Empty<ISeries>();

    [ObservableProperty]
    public partial Axis[] MonthXAxes { get; set; } = Array.Empty<Axis>();

    public ReportsViewModel()
    {
        Refresh();
    }

    [RelayCommand]
    private void Refresh()
    {
        var from = (FromDate ?? DateTime.Today).Date;
        var to = (ToDate ?? DateTime.Today).Date;

        if (from > to)
        {
            Summary = "Дата «з» не може бути пізніше за дату «по»";
            return;
        }

        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        var fuel = db.FuelRecords.ToList();
        var service = db.ServiceRecords.ToList();
        var other = db.OtherExpenses.ToList();

        var categories = ReportService.GetCategoryTotals(fuel, service, other, from, to);
        var months = ReportService.GetMonthlyTotals(fuel, service, other, from, to);
        var total = ReportService.GrandTotal(fuel, service, other, from, to);

        Summary = $"Разом за період: {total:N2} грн  (" +
                  string.Join(", ", categories.Select(c => $"{c.Category}: {c.Total:N2}")) + ")";

        CategorySeries = categories
            .Select(c => (ISeries)new PieSeries<decimal> { Name = c.Category, Values = new[] { c.Total } })
            .ToArray();

        MonthSeries = new ISeries[]
        {
            new ColumnSeries<decimal> { Name = "Витрати, грн", Values = months.Select(m => m.Total).ToArray() }
        };

        MonthXAxes = new[]
        {
            new Axis { Labels = months.Select(m => $"{m.Month:00}.{m.Year}").ToArray() }
        };
    }
}