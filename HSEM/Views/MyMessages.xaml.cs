using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MyMessages : ContentPage
{
	public MyMessages()
	{
		InitializeComponent();
	}

    private async void OnMessageSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var selectedMessage = e.CurrentSelection[0] as MessageDto;
            if (selectedMessage == null) return;

            // فتح صفحة قراءة الرسالة
            await Navigation.PushAsync(new ReadMessagePage(selectedMessage));

            // إلغاء تحديد العنصر
            ((CollectionView)sender).SelectedItem = null;
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