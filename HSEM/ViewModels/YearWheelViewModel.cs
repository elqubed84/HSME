using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace HSEM.ViewModels
{
    public class YearWheelViewModel : BindableObject
    {
        public ObservableCollection<int> Years { get; } = new();

        private int _selectedYearIndex;
        public int SelectedYearIndex
        {
            get => _selectedYearIndex;
            set
            {
                if (_selectedYearIndex != value)
                {
                    _selectedYearIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedYear));
                }
            }
        }

        public int SelectedYear => (SelectedYearIndex >= 0 && SelectedYearIndex < Years.Count)
                                   ? Years[SelectedYearIndex] : DateTime.Now.Year;

        public ICommand IncreaseIndexCommand { get; }
        public ICommand DecreaseIndexCommand { get; }

        public YearWheelViewModel()
        {
            int current = DateTime.Now.Year;
            for (int i = 0; i < 10; i++) // أمثلة: آخر 10 سنين
                Years.Add(current - i);

            SelectedYearIndex = 0; // يختار السنة الحالية افتراضياً

            IncreaseIndexCommand = new Command(() =>
            {
                if (SelectedYearIndex > 0) SelectedYearIndex--;
            });

            DecreaseIndexCommand = new Command(() =>
            {
                if (SelectedYearIndex < Years.Count - 1) SelectedYearIndex++;
            });
        }
    }
}
