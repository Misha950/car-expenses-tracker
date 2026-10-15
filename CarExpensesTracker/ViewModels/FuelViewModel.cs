using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarExpensesTracker.Data;
using CarExpensesTracker.Models;

namespace CarExpensesTracker.ViewModels
{
    public partial class FuelViewModel : ViewModelBase
    {
        [ObservableProperty]
        private decimal _liters;

        [ObservableProperty]
        private decimal _pricePerLiter;

        [ObservableProperty]
        private DateTime _date = DateTime.Now;

        public ObservableCollection<FuelRecord> FuelRecords { get; set; } = new();

        public ICommand SaveCommand { get; }

        public FuelViewModel()
        {
            LoadRecords();
            SaveCommand = new RelayCommand(SaveRecord);
        }

        private void LoadRecords()
        {
            FuelRecords.Clear();
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                var records = db.FuelRecords.ToList();
                foreach (var record in records)
                {
                    FuelRecords.Add(record);
                }
            }
        }

        private void SaveRecord()
        {
            if (Liters <= 0 || PricePerLiter <= 0) return;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                var newRecord = new FuelRecord
                {
                    Date = Date,
                    Liters = Liters,
                    PricePerLiter = PricePerLiter
                };

                db.FuelRecords.Add(newRecord);
                db.SaveChanges();
            }

            Liters = 0;
            PricePerLiter = 0;
            LoadRecords();
        }
    }
}