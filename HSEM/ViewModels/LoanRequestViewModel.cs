using System;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HSEM.Interfaces;
using HSEM.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using HSEM.Services;
using System.Globalization;

namespace HSEM.ViewModels
{
    public class LoanRequestViewModel : BindableObject
    {
        private readonly ILoanService _loanService;
        private readonly IPopupService _alert;

        public ICommand SubmitCommand { get; }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (HasMoreThanTwoDecimals(value))
                {
                    // نحد من رقمين عشريين
                    value = Math.Round(value, 2);
                }
                _amount = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private int _installments;
        public int Installments
        {
            get => _installments;
            set
            {
                _installments = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        public LoanRequestViewModel()
        {
            _loanService = new LoanService();
            _alert = new PopupService();

            SubmitCommand = new Command(async () => await SubmitAsync(),
                                       CanSubmit);
        }

        private bool CanSubmit()
        {
            return !IsBusy && Amount > 0 && Installments > 0;
        }

        private bool HasMoreThanTwoDecimals(decimal value)
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            if (!str.Contains(".")) return false;
            return str.Split('.')[1].Length > 2;
        }

        private async Task SubmitAsync()
        {
            if (!CanSubmit())
            {
                await _alert.ShowAlertAsync("خطأ", "تأكد من إدخال مبلغ صالح وعدد أقساط أكبر من صفر", "موافق");
                return;
            }

            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    await _alert.ShowAlertAsync("خطأ", "يرجى تسجيل الدخول أولاً", "موافق");
                    return;
                }

                var dto = new LoanRequestDto { Amount = Amount, Installments = Installments };
                var (success, message) = await _loanService.SubmitAsync(dto, token);

                await _alert.ShowAlertAsync(success ? "نجاح" : "خطأ", message, "موافق");

                if (success)
                {
                    Amount = 0;
                    Installments = 0;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
