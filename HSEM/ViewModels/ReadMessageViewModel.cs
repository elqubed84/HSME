using HSEM.Models;
using HSEM.Views;
using System;
using System.Windows.Input;

namespace HSEM.ViewModels;

public class ReadMessageViewModel : BaseViewModel
{
    public MessageDto Message { get; private set; }

    public bool HasParentMessage => Message?.ParentMessage != null;

    public MessageDto ParentMessage => Message?.ParentMessage;

    public bool CanReply => true; // يمكن تفعيل أو تعطيل حسب منطقك

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand ReplyCommand { get; }
    public ICommand BackCommand { get; }

    public ReadMessageViewModel(MessageDto message)
    {
        LoadMessage(message);

        ReplyCommand = new Command(OnReply);
        BackCommand = new Command(OnBack);
    }

    public void LoadMessage(MessageDto message)
    {
        if (message == null) return;

        IsBusy = true;

        Message = message;

        if (!Message.IsRead)
        {
            Message.IsRead = true;
            Message.ReadAt = DateTime.Now;
        }

        IsBusy = false;
        OnPropertyChanged(nameof(Message));
        OnPropertyChanged(nameof(HasParentMessage));
        OnPropertyChanged(nameof(ParentMessage));
    }

    private async void OnReply()
    {
        if (Message == null) return;

        var page = new ComposeMessagePage(
            receiverId: Message.OtherParty.Id,
            parentMessageId: Message.Id
        );

        var vm = page.BindingContext as ComposeMessageViewModel;

        if (vm != null)
        {
            vm.SelectedReceiver = new MessageUserDto
            {
                Id = Message.OtherParty.Id,
                FullName = Message.OtherParty.FullName
            };

            vm.Subject = $"رد: {Message.Subject}";
        }

        // ✅ نفس طريقة التنقل الصح
        if (App.Current.MainPage is FlyoutPage flyoutPage &&
            flyoutPage.Detail is NavigationPage navPage)
        {
            await navPage.PushAsync(page);
            flyoutPage.IsPresented = false; // يقفل القائمة
        }
    }
    private async void OnBack()
    {
        var masterDetail = App.Current.MainPage as FlyoutPage;
        var navigationPage = masterDetail.Detail as NavigationPage;
        if (masterDetail.Detail is NavigationPage navigationPage2)
        {
            await navigationPage2.PopAsync();
        }
    }
}