
using HSEM.Helper;
using HSEM.Models;
using HSEM.ViewModels;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace HSEM.Views;

public partial class NyLeaveRequests : ContentPage
{
    public NyLeaveRequests()
	{
		InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MyRequestViewModel vm)
        {
            vm.LoadCommand.Execute(null);
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