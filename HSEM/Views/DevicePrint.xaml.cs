using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class DevicePrint : ContentPage
{
	public DevicePrint()
	{
		InitializeComponent();
	}
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is DevicePrintViewModel vm)
        {
            vm.SearchText = e.NewTextValue;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DevicePrintViewModel vm)
        {
            await vm.InitializeAsync();
        }
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