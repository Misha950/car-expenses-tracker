using System;

namespace CarExpensesTracker.Models
{
    public class ServiceRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int Mileage { get; set; }
        public DateTime? NextServiceDate { get; set; }
    }
}