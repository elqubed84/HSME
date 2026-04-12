using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MyPermissions : ContentPage
{
	public MyPermissions()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MyPermissionsViewModel vm)
            vm.LoadCommand.Execute(null);
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