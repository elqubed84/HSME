using HSEM.Helper;
using HSEM.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using HSEM.Services;
using HSEM.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace HSEM.ViewModels
{
    public class PermissionRequestViewModel : BindableObject
    {
        private readonly IPermissionRequestService _service;
        private readonly IPopupService _alert;
        private readonly IOfflineQueueService _queue;

        //public ObservableCollection<EnumItem<PermissionType>> PermissionTypes { get; }
        //public ObservableCollection<EnumItem<PermissionScope>> PermissionScopes { get; }
        public ObservableCollection<EnumItem<PermissionType>> PermissionTypes { get; }
    = new ObservableCollection<EnumItem<PermissionType>>();

        public ObservableCollection<EnumItem<PermissionScope>> PermissionScopes { get; }
            = new ObservableCollection<EnumItem<PermissionScope>>();


        public ICommand SubmitCommand { get; }
        public ICommand OpenTypePopupCommand { get; }
        public ICommand SelectTypeCommand { get; }

        private bool _isTypePopupOpen;
        public bool IsTypePopupOpen
        {
            get => _isTypePopupOpen;
            set { _isTypePopupOpen = value; OnPropertyChanged(); }
        }

        #region Constructors

        // Default constructor for XAML / Preview
        public PermissionRequestViewModel()
        {
            _service = new PermissionRequestService();  // dummy
            _alert = new PopupService();                // dummy
            _queue = new OfflineQueueService();        // dummy

            InitCollections();
            SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
            OpenTypePopupCommand = new Command(() => IsTypePopupOpen = true);

            SelectTypeCommand = new Command<EnumItem<PermissionType>>(item =>
            {
                SelectedType = item;
                IsTypePopupOpen = false;
            });

        }

        // Constructor for DI
        public PermissionRequestViewModel(IPermissionRequestService service,
                                          IPopupService alert,
                                          IOfflineQueueService queue)
        {
            _service = service;
            _alert = alert;
            _queue = queue;

            InitCollections();
            SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
        }

        #endregion

        private void InitCollections()
        {
            // أفرغ المحتوى لو موجود
            PermissionTypes.Clear();
            foreach (var e in Enum.GetValues(typeof(PermissionType)).Cast<PermissionType>())
                PermissionTypes.Add(new EnumItem<PermissionType>(e, e.GetDisplayName()));

            PermissionScopes.Clear();
            foreach (var e in Enum.GetValues(typeof(PermissionScope)).Cast<PermissionScope>())
                PermissionScopes.Add(new EnumItem<PermissionScope>(e, e.GetDisplayName()));

            SelectedType = PermissionTypes.FirstOrDefault();
            SelectedScope = PermissionScopes.FirstOrDefault();
        }


        #region Properties

        private EnumItem<PermissionType> _selectedType;
        public EnumItem<PermissionType> SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        private EnumItem<PermissionScope> _selectedScope;
        public EnumItem<PermissionScope> SelectedScope
        {
            get => _selectedScope;
            set { _selectedScope = value; OnPropertyChanged(); }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); OnPropertyChanged(nameof(StartDateTime)); OnPropertyChanged(nameof(EndDateTime)); }
        }

        private TimeSpan _startTime = TimeSpan.FromHours(9);
        public TimeSpan StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(StartDateTime)); }
        }

        private TimeSpan _endTime = TimeSpan.FromHours(17);
        public TimeSpan EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(EndDateTime)); }
        }

        public DateTime StartDateTime => Date.Date + StartTime;
        public DateTime EndDateTime => Date.Date + EndTime;

        private string _managerNotes = string.Empty;
        public string ManagerNotes
        {
            get => _managerNotes;
            set { _managerNotes = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); ((Command)SubmitCommand).ChangeCanExecute(); }
        }

        private const double MaxHours = 8;
        private const int MaxRetry = 3;

        #endregion

        #region Methods

        private async Task SubmitAsync()
        {
            if (IsBusy) return;

            if (!Validate(out var error))
            {
                await _alert.ShowAlertAsync("خطأ", error, "موافق");
                return;
            }

            try
            {
                IsBusy = true;
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    await _alert.ShowAlertAsync("خطأ", "يرجى تسجيل الدخول أولاً", "موافق");
                    return;
                }

                var dto = new PermissionRequestDto
                {
                    Type = SelectedType.Value,
                    Scope = SelectedScope.Value,
                    StartDateTime = StartDateTime,
                    EndDateTime = EndDateTime,
                    ManagerNotes = ManagerNotes
                };

                var result = await TrySubmitWithRetry(dto, token);

                if (!result.success && result.isOffline)
                {
                    await _queue.EnqueueAsync(dto);
                    await _alert.ShowAlertAsync("تنبيه", "لا يوجد اتصال، سيتم الإرسال تلقائيًا عند توفر الإنترنت", "موافق");
                    return;
                }

                await _alert.ShowAlertAsync(result.success ? "نجاح" : "خطأ", result.message, "موافق");

                if (result.success)
                    Reset();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<(bool success, bool isOffline, string message)> TrySubmitWithRetry(PermissionRequestDto dto, string token)
        {
            for (int i = 1; i <= MaxRetry; i++)
            {
                var (success, message) = await _service.SubmitAsync(dto, token);

                if (success) return (true, false, message);

                if (message.Contains("اتصال") || message.Contains("connection"))
                    await Task.Delay(1000 * i * i);
                else
                    return (false, false, message);
            }

            return (false, true, "تعذر الاتصال بالخادم");
        }

        private bool Validate(out string error)
        {
            error = "";

            if (EndDateTime <= StartDateTime)
            {
                error = "وقت الانتهاء يجب أن يكون بعد وقت البداية";
                return false;
            }

            var duration = (EndDateTime - StartDateTime).TotalHours;
            if (duration > MaxHours)
            {
                error = $"لا يمكن طلب إذن لأكثر من {MaxHours} ساعات";
                return false;
            }

            if (SelectedType == null || SelectedScope == null)
            {
                error = "يرجى اختيار نوع ونطاق الإذن";
                return false;
            }

            return true;
        }

        public async Task FlushQueueAsync(string token)
        {
            var pending = await _queue.DequeueAllAsync();

            foreach (var item in pending)
            {
                var (success, _) = await _service.SubmitAsync(item.Request, token);
                if (!success) break;
            }
        }

        private void Reset()
        {
            Date = DateTime.Today;
            StartTime = TimeSpan.FromHours(9);
            EndTime = TimeSpan.FromHours(17);
            ManagerNotes = string.Empty;
            SelectedType = PermissionTypes.First();
            SelectedScope = PermissionScopes.First();
        }

        #endregion
    }
}
