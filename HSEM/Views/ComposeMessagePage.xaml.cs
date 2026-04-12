using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class ComposeMessagePage : ContentPage
{
    public ComposeMessagePage(string receiverId = null, int? parentMessageId = null)
    {
        InitializeComponent();
        BindingContext = new ComposeMessageViewModel(receiverId, parentMessageId);
    }
    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await NavigationHelper.PopCurrentPageAsync();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        NavigationHelper.ToggleFlyoutMenu();
    }

    protected override bool OnBackButtonPressed()
    {
        return NavigationHelper.HandleBackButton();
    }

}