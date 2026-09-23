using System;

namespace CarExpensesTracker.Models
{
    public class FuelRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public decimal TotalCost => Liters * PricePerLiter;
        public int Mileage { get; set; }
    }
}