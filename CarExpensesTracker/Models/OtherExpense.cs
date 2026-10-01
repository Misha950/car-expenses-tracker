using System;

namespace CarExpensesTracker.Models
{
    public class OtherExpense
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}