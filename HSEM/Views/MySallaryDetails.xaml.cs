using HSEM.Helper;
using HSEM.Services;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MySallaryDetails : ContentPage
{
    public MySallaryDetails(int year, int month, string monthName)
    {
        InitializeComponent();
        BindingContext = new MySalaryDetailsViewModel(year, month,monthName);
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MySallary());
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