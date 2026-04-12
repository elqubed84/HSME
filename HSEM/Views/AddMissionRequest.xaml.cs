using HSEM.Helper;

namespace HSEM.Views;

public partial class AddMissionRequest : ContentPage
{
	public AddMissionRequest()
	{
		InitializeComponent();
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