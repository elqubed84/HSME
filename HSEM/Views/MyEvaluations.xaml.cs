using HSEM.Helper;
using HSEM.Models;

namespace HSEM.Views;

public partial class MyEvaluations : ContentPage
{
	public MyEvaluations()
	{
		InitializeComponent();
	}
    //private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    if (e.CurrentSelection.FirstOrDefault() is MyEvaluationDto selected)
    //    {
    //        await Navigation.PushAsync(new EmployeeEvaluationDetailsPage(selected.Month, selected.Year));
    //        ((CollectionView)sender).SelectedItem = null;
    //    }
    //}


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