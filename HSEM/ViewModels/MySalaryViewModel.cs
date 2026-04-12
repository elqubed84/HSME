using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using HSEM.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MySalaryViewModel : BindableObject
    {
        private readonly IPopupService _alertService;
        private readonly INavigationService _navigationService;
        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<MonthItem> Months { get; } = new();

        private int _selectedYearIndex;
        public int SelectedYearIndex
        {
            get => _selectedYearIndex;
            set
            {
                _selectedYearIndex = value;
                if (value >= 0 && value < Years.Count)
                    SelectedYear = Years[value];
                OnPropertyChanged();
            }
        }

        private int _selectedMonthIndex;
        public int SelectedMonthIndex
        {
            get => _selectedMonthIndex;
            set
            {
                _selectedMonthIndex = value;
                if (value >= 0 && value < Months.Count)
                    SelectedMonth = Months[value];
                OnPropertyChanged();
            }
        }

        private int _selectedYear;
        public int SelectedYear { get; set; }

        private MonthItem _selectedMonth;
        public MonthItem SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }
        private readonly IPopupService _alert;
        public ICommand NavigateToDetailsCommand { get; }

        public MySalaryViewModel()
        {
            _alert = new PopupService();
            _navigationService = new NavigationService();

            InitYears();
            InitMonths();

            NavigateToDetailsCommand = new Command(async () => await NavigateToDetailsAsync());
        }

        private void InitYears()
        {
            var current = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
                Years.Add(current - i);

            SelectedYear = current;
            SelectedYearIndex = 0;
        }

        private void InitMonths()
        {
            Months.Add(new MonthItem(1, "يناير"));
            Months.Add(new MonthItem(2, "فبراير"));
            Months.Add(new MonthItem(3, "مارس"));
            Months.Add(new MonthItem(4, "إبريل"));
            Months.Add(new MonthItem(5, "مايو"));
            Months.Add(new MonthItem(6, "يونيو"));
            Months.Add(new MonthItem(7, "يوليو"));
            Months.Add(new MonthItem(8, "أغسطس"));
            Months.Add(new MonthItem(9, "سبتمبر"));
            Months.Add(new MonthItem(10, "أكتوبر"));
            Months.Add(new MonthItem(11, "نوفمبر"));
            Months.Add(new MonthItem(12, "ديسمبر"));

            SelectedMonth = Months[DateTime.Now.Month - 1];
            SelectedMonthIndex = DateTime.Now.Month - 1;
        }

        private async Task NavigateToDetailsAsync()
        {
            if (SelectedMonth == null)
            {
                await _alertService.ShowAlertAsync("تنبيه", "اختر الشهر أولاً", "موافق");
                return;
            }

            await _navigationService.GoToPayrollDetailsAsync(SelectedYear, SelectedMonth.Number, SelectedMonth.Name);
        }
    }
}
