using HSEM.Interfaces;
using HSEM.Services;
using HSEM.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MyMessagesViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient = HttpClientFactory.Instance;
        private readonly IPopupService _popup;

        public ObservableCollection<MessageDto> Messages { get; set; } = new();

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set => SetProperty(ref _unreadCount, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand OpenMessageCommand { get; }

        public MyMessagesViewModel()
        {
            _popup = new PopupService();

            LoadCommand = new Command(async () => await LoadMessages());
            OpenMessageCommand = new Command<MessageDto>(async (msg) => await OpenMessage(msg));

            Task.Run(async () => await LoadMessages());
        }

        private async Task OpenMessage(MessageDto message)
        {
            if (message == null) return;

            var page = new ReadMessagePage(message);

            // ⚡ الحصول على NavigationPage من FlyoutPage
            if (App.Current.MainPage is FlyoutPage flyoutPage &&
                flyoutPage.Detail is NavigationPage navPage)
            {
                await navPage.PushAsync(page);
            }
            else
            {
                // لو مش موجود، نلف الصفحة بـ NavigationPage مؤقت
                await Application.Current.MainPage.Navigation.PushAsync(new NavigationPage(page));
            }
        }

        public async Task LoadMessages()
        {
            try
            {
                IsLoading = true;

                var token = await SecureStorage.Default.GetAsync("AccessToken");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var result = await _httpClient.GetFromJsonAsync<InboxResponse>(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/messages/inbox");

                Messages.Clear();

                if (result?.messages != null && result.messages.Count > 0)
                {
                    foreach (var msg in result.messages)
                        Messages.Add(msg);

                    UnreadCount = result.unreadCount;
                     _popup.ShowSuccessToast("تم تحميل الرسائل بنجاح ✅");
                }
                else
                {
                    UnreadCount = 0;
                     _popup.ShowSuccessToast("لا توجد رسائل حالياً.");
                }
            }
            catch (HttpRequestException ex)
            {
                 _popup.ShowSuccessToast("تعذر الوصول للسيرفر ❌\n" + ex.Message);
            }
            catch (Exception ex)
            {
                 _popup.ShowSuccessToast( "حدث خطأ أثناء تحميل الرسائل: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // ======= النماذج =======
    public class InboxResponse
    {
        public List<MessageDto> messages { get; set; }
        public int unreadCount { get; set; }
    }

    public class MessageUserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public string Picture { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? ParentMessageId { get; set; }
        public MessageUserDto OtherParty { get; set; }
        public MessageDto ParentMessage { get; set; }
    }
}