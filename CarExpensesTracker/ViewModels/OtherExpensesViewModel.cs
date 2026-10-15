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
    public partial class OtherExpensesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _type = string.Empty;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private DateTime _date = DateTime.Now;

        public ObservableCollection<OtherExpense> OtherExpenses { get; set; } = new();

        public ICommand SaveCommand { get; }

        public OtherExpensesViewModel()
        {
            LoadExpenses();
            SaveCommand = new RelayCommand(SaveExpense);
        }

        private void LoadExpenses()
        {
            OtherExpenses.Clear();
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                var list = db.OtherExpenses.ToList();
                foreach (var item in list)
                {
                    OtherExpenses.Add(item);
                }
            }
        }

        private void SaveExpense()
        {
            if (string.IsNullOrWhiteSpace(Type) || Amount <= 0) return;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                var newExpense = new OtherExpense
                {
                    Type = Type,
                    Amount = Amount,
                    Date = Date
                };

                db.OtherExpenses.Add(newExpense);
                db.SaveChanges();
            }

            Type = string.Empty;
            Amount = 0;
            LoadExpenses();
        }
    }
}