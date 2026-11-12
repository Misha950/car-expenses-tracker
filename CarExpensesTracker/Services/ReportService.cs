using System;
using System.Collections.Generic;
using System.Linq;
using CarExpensesTracker.Models;

namespace CarExpensesTracker.Services;

public record CategoryTotal(string Category, decimal Total);

public record MonthTotal(int Year, int Month, decimal Total);

public static class ReportService
{
    public static decimal TotalFuel(IEnumerable<FuelRecord> fuel, DateTime from, DateTime to) =>
        fuel.Where(r => InPeriod(r.Date, from, to)).Sum(r => r.TotalCost);

    public static decimal TotalService(IEnumerable<ServiceRecord> service, DateTime from, DateTime to) =>
        service.Where(r => InPeriod(r.Date, from, to)).Sum(r => r.Cost);

    public static decimal TotalOther(IEnumerable<OtherExpense> other, DateTime from, DateTime to) =>
        other.Where(r => InPeriod(r.Date, from, to)).Sum(r => r.Amount);

    public static decimal GrandTotal(
        IEnumerable<FuelRecord> fuel,
        IEnumerable<ServiceRecord> service,
        IEnumerable<OtherExpense> other,
        DateTime from, DateTime to) =>
        TotalFuel(fuel, from, to) + TotalService(service, from, to) + TotalOther(other, from, to);

    public static List<CategoryTotal> GetCategoryTotals(
        IEnumerable<FuelRecord> fuel,
        IEnumerable<ServiceRecord> service,
        IEnumerable<OtherExpense> other,
        DateTime from, DateTime to) =>
        new()
        {
            new CategoryTotal("Пальне", TotalFuel(fuel, from, to)),
            new CategoryTotal("Ремонти та ТО", TotalService(service, from, to)),
            new CategoryTotal("Інше", TotalOther(other, from, to)),
        };

    public static List<MonthTotal> GetMonthlyTotals(
        IEnumerable<FuelRecord> fuel,
        IEnumerable<ServiceRecord> service,
        IEnumerable<OtherExpense> other,
        DateTime from, DateTime to)
    {
        var result = new List<MonthTotal>();
        var month = new DateTime(from.Year, from.Month, 1);
        var last = new DateTime(to.Year, to.Month, 1);

        while (month <= last)
        {
            var start = month;
            var end = month.AddMonths(1).AddDays(-1);
            var total = GrandTotal(fuel, service, other, start, end);
            result.Add(new MonthTotal(month.Year, month.Month, total));
            month = month.AddMonths(1);
        }

        return result;
    }

    private static bool InPeriod(DateTime date, DateTime from, DateTime to) =>
        date.Date >= from.Date && date.Date <= to.Date;
}