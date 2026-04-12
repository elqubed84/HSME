using CommunityToolkit.Maui.Views;
using HSEM.Interfaces;
using HSEM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class CreateCompensatoryDayViewModel : BindableObject
    {
        private readonly IApiService _api;
        private readonly IPopupService _popup;

        private DateTime _workDate = DateTime.Today;
        public DateTime WorkDate
        {
            get => _workDate;
            set { _workDate = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }


        private double _defaultRate;
        public double DefaultRate
        {
            get => _defaultRate;
            set { _defaultRate = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand LoadDefaultsCommand { get; }
        public ICommand SubmitCommand { get; }

        public CreateCompensatoryDayViewModel()
        {
            _api = new ApiService();
            _popup = new PopupService();
            LoadDefaultsCommand = new Command(async () => await LoadDefaults());
            SubmitCommand = new Command(async () => await Submit());
        }

        async Task LoadDefaults()
        {
            try
            {
                var res = await _api.GetAsync("https://elnagarygroup-001-site1.ktempurl.com/api/CompensatoryDays/defaults");

                if (!res.IsSuccessStatusCode)
                {
                     _popup.ShowSuccessToast( $"Status: {res.StatusCode}");
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();
                var obj = JsonSerializer.Deserialize<JsonElement>(json);

                DefaultRate = obj.GetProperty("rate").GetDouble();
            }
            catch (HttpRequestException ex)
            {
                 _popup.ShowSuccessToast( "فشل الاتصال بالسيرفر ❌\n" + ex.Message);
            }
            catch (Exception ex)
            {
                 _popup.ShowSuccessToast( ex.Message);
            }
        }

        async Task Submit()
        {
            if (WorkDate == default)
            {
                 _popup.ShowSuccessToast( "من فضلك اختر تاريخ العمل.");
                return;
            }
            if (WorkDate.Date > DateTime.Today)
            {
                 _popup.ShowSuccessToast( "لا يمكن اختيار تاريخ مستقبلي.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Description))
            {
                 _popup.ShowSuccessToast( "من فضلك اكتب وصف العمل.");
                return;
            }

            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var dto = new
                {
                    WorkDate = WorkDate,
                    Description = Description
                };

                await _api.PostAsync("https://elnagarygroup-001-site1.ktempurl.com/api/CompensatoryDays", dto);
                 _popup.ShowSuccessToast("تم إرسال الطلب بنجاح");
                WorkDate = DateTime.Today;
                Description = string.Empty;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}
