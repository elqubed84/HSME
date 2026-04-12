using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace HSEM.ViewModels
{
    public class MissionRequestViewModel : BindableObject
    {
        private readonly IMissionService _missionService;
        private readonly IPopupService _alert;

        public ICommand SubmitCommand { get; }

        #region Properties

        private DateTime _missionDate = DateTime.Today;
        public DateTime MissionDate
        {
            get => _missionDate;
            set
            {
                _missionDate = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private TimeSpan _fromTime;
        public TimeSpan FromTime
        {
            get => _fromTime;
            set
            {
                _fromTime = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private TimeSpan _toTime;
        public TimeSpan ToTime
        {
            get => _toTime;
            set
            {
                _toTime = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private string _location;
        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
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

        #endregion

        public MissionRequestViewModel()
        {
            _missionService = new MissionService();
            _alert = new PopupService();

            SubmitCommand = new Command(
                async () => await SubmitAsync(),
                CanSubmit);
        }

        #region Validation

        private bool CanSubmit()
        {
            return !IsBusy
                   && MissionDate != default
                   && FromTime < ToTime
                   && !string.IsNullOrWhiteSpace(Location)
                   && !string.IsNullOrWhiteSpace(Description);
        }

        #endregion

        #region Submit

        private async Task SubmitAsync()
        {
            if (!CanSubmit())
            {
                await _alert.ShowAlertAsync(
                    "خطأ",
                    "تأكد من إدخال جميع البيانات بشكل صحيح",
                    "موافق");
                return;
            }

            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    await _alert.ShowAlertAsync(
                        "خطأ",
                        "يرجى تسجيل الدخول أولاً",
                        "موافق");
                    return;
                }

                var dto = new MissionRequestDto
                {
                    MissionDate = MissionDate,
                    FromTime = FromTime,
                    ToTime = ToTime,
                    Location = Location,
                    Description = Description
                };

                var (success, message) =
                    await _missionService.SubmitAsync(dto, token);

                await _alert.ShowAlertAsync(
                    success ? "نجاح" : "خطأ",
                    message,
                    "موافق");

                if (success)
                {
                    ResetForm();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Helpers

        private void ResetForm()
        {
            MissionDate = DateTime.Today;
            FromTime = TimeSpan.Zero;
            ToTime = TimeSpan.Zero;
            Location = string.Empty;
            Description = string.Empty;
        }

        #endregion
    }
}