using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HSEM.Interfaces;
using HSEM.Services;
using Microsoft.Maui.Controls;

namespace HSEM.ViewModels
{
    public class MyAttendanceViewModel : BindableObject
    {
        private readonly IPopupService _alertService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<MonthItem> Months { get; } = new();

        // Commands
        public ICommand ShowCommand { get; }
        public ICommand ConfirmYearCommand { get; }
        public ICommand ConfirmMonthCommand { get; }

        #region Animation trigger
        private int _selectionChangeTick;
        public int SelectionChangeTick
        {
            get => _selectionChangeTick;
            set
            {
                _selectionChangeTick = value;
                OnPropertyChanged();
            }
        }
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        private void TriggerSelectionAnimation()
        {
            SelectionChangeTick++;
        }
        #endregion

        #region Constructors
        public MyAttendanceViewModel()
        {
            _alertService = new PopupService();
            _navigationService = new NavigationService();

            InitYears();
            InitMonths();

            ShowCommand = new Command(async () => await ShowAsync());
            ConfirmYearCommand = new Command(() => TriggerSelectionAnimation());
            ConfirmMonthCommand = new Command(() => TriggerSelectionAnimation());
        }
        #endregion

        #region Selected Properties
        private int _selectedYearIndex;
        public int SelectedYearIndex
        {
            get => _selectedYearIndex;
            set
            {
                if (_selectedYearIndex != value)
                {
                    _selectedYearIndex = value;
                    SelectedYear = Years[_selectedYearIndex];
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedMonthIndex;
        public int SelectedMonthIndex
        {
            get => _selectedMonthIndex;
            set
            {
                if (_selectedMonthIndex != value)
                {
                    _selectedMonthIndex = value;
                    SelectedMonth = Months[_selectedMonthIndex];
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedYearMonthText));
                    TriggerSelectionAnimation();
                }
            }
        }

        private MonthItem _selectedMonth;
        public MonthItem SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMonthName));
                    OnPropertyChanged(nameof(SelectedYearMonthText));
                    TriggerSelectionAnimation();
                }
            }
        }

        public string SelectedMonthName => SelectedMonth?.Name ?? "";
        public string SelectedYearMonthText => SelectedMonth == null ? "اختر الشهر والسنة" : $"مؤثرات شهر {SelectedMonth.Name} لسنة {SelectedYear}";
        #endregion

        #region Init
        private void InitYears()
        {
            var current = DateTime.Now.Year;

            for (int i = 0; i < 5; i++)
                Years.Add(current - i);

            var index = Years.IndexOf(current);
            if (index < 0)
                index = 0;

            SelectedYearIndex = index;

            // تأكيد ضبط السنة نفسها
            SelectedYear = Years[index];
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

            var currentMonth = DateTime.Now.Month;

            var monthIndex = Months
                .Select((m, i) => new { m.Number, Index = i })
                .First(x => x.Number == currentMonth).Index;

            SelectedMonthIndex = monthIndex;
            SelectedMonth = Months[monthIndex];
        }

        #endregion

        #region Actions
        private async Task ShowAsync()
        {
            if (SelectedMonth == null)
            {
                await _alertService.ShowAlertAsync("تنبيه", "اختر الشهر أولاً", "موافق");
                return;
            }

            await _navigationService.GoToAttendanceDetailsAsync(
                SelectedYear,
                SelectedMonth.Number);
        }
        #endregion
    }

    public class MonthItem
    {
        public int Number { get; }
        public string Name { get; }

        public MonthItem(int number, string name)
        {
            Number = number;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
