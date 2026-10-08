using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarExpensesTracker.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; } = new FuelViewModel();

    [RelayCommand]
    private void ShowFuel() => CurrentPage = new FuelViewModel();

    [RelayCommand]
    private void ShowRepairs() => CurrentPage = new RepairsViewModel();

    [RelayCommand]
    private void ShowOtherExpenses() => CurrentPage = new OtherExpensesViewModel();

    [RelayCommand]
    private void ShowReports() => CurrentPage = new ReportsViewModel();
}