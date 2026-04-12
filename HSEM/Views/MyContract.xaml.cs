using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MyContract : ContentPage
{
	public MyContract()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var vm = BindingContext as MyContractViewModel;
        if (vm != null && vm.Contracts.Count == 0)
            await vm.LoadContractsAsync();
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