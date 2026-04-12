using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class ReadMessagePage : ContentPage
{
    public ReadMessagePage(MessageDto message)
    {
        InitializeComponent();
        BindingContext = new ReadMessageViewModel(message);
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