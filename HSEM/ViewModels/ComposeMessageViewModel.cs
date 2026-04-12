using HSEM.Helper;
using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels;

public class ComposeMessageViewModel : BaseViewModel
{
    private readonly HttpClient _httpClient = HttpClientFactory.Instance;
    private readonly IPopupService _popup;

    public ObservableCollection<MessageUserDto> Receivers { get; set; } = new();

    private MessageUserDto _selectedReceiver;
    public MessageUserDto SelectedReceiver
    {
        get => _selectedReceiver;
        set => SetProperty(ref _selectedReceiver, value);
    }

    private string _subject;
    public string Subject
    {
        get => _subject;
        set => SetProperty(ref _subject, value);
    }

    private string _body;
    public string Body
    {
        get => _body;
        set => SetProperty(ref _body, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // 👇 مهم
    private bool _isReplyMode;
    public bool IsReplyMode => _isReplyMode;

    public string ReceiverId { get; set; }
    public int? ParentMessageId { get; set; }
    public bool ShowReceiverPicker => !_isReplyMode;
    public bool ShowReceiverLabel => _isReplyMode;
    public ICommand SendCommand { get; }
    public ICommand CancelCommand { get; }

    public ComposeMessageViewModel(string receiverId = null, int? parentMessageId = null)
    {
        _popup = new PopupService();

        ReceiverId = receiverId;
        ParentMessageId = parentMessageId;

        _isReplyMode = !string.IsNullOrEmpty(receiverId);

        SendCommand = new Command(async () => await SendMessage());
        CancelCommand = new Command(async () => await Cancel());

        // 👇 لو مش رد → حمل الموظفين
        if (!_isReplyMode)
            Task.Run(async () => await LoadReceivers());
        else
            InitReplyMode();
    }

    // ✅ تحميل الموظفين (فقط في الوضع العادي)
    private async Task LoadReceivers()
    {
        try
        {
            IsBusy = true;

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var users = await _httpClient.GetFromJsonAsync<List<MessageUserDto>>(
                "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/messages/users");

            Receivers.Clear();

            if (users != null)
            {
                foreach (var user in users)
                    Receivers.Add(user);
            }
        }
        catch (Exception ex)
        {
             _popup.ShowSuccessToast("خطأ \n"+ "تعذر تحميل المستقبلين ❌\n" + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ✅ وضع الرد (بدون API)
    private void InitReplyMode()
    {
        // بنعمل Receiver وهمي فقط للعرض
        SelectedReceiver = new MessageUserDto
        {
            Id = ReceiverId,
            FullName = "جارٍ تحديد المستلم..."
        };

        // عنوان افتراضي
        Subject = "رد على رسالة";
    }

    // ✅ إرسال الرسالة
    private async Task SendMessage()
    {
        // ✅ تحديد الـ ID النهائي
        var finalReceiverId = ReceiverId ?? SelectedReceiver?.Id;

        // 🚫 حماية أساسية
        if (string.IsNullOrWhiteSpace(finalReceiverId))
        {
             _popup.ShowSuccessToast( "لا يمكن الإرسال بدون تحديد مستلم ❌");
            return;
        }

        if (string.IsNullOrWhiteSpace(Subject))
        {
             _popup.ShowSuccessToast( "ادخل الموضوع");
            return;
        }

        if (string.IsNullOrWhiteSpace(Body))
        {
             _popup.ShowSuccessToast( "اكتب محتوى الرسالة");
            return;
        }

        try
        {
            IsBusy = true;

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var messageToSend = new
            {
                ReceiverId = finalReceiverId, // 👈 استخدم القيمة الآمنة
                Subject = Subject.Trim(),
                Body = Body.Trim(),
                ParentMessageId = ParentMessageId
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/messages/compose",
                messageToSend
            );

            if (response.IsSuccessStatusCode)
            {
                 _popup.ShowSuccessToast("تم إرسال الرسالة ✅");
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                 _popup.ShowSuccessToast( $"تعذر إرسال الرسالة ❌\n{content}");
            }
        }
        catch (Exception ex)
        {
             _popup.ShowSuccessToast( "حدث خطأ أثناء الإرسال:\n" + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ✅ رجوع
    private async Task Cancel()
    {
        await NavigationHelper.PopCurrentPageAsync();
    }
}