using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MyAttendance : ContentPage
{
	public MyAttendance()
	{
		InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }
    private void OnBindingContextChanged(object sender, EventArgs e)
    {
        if (BindingContext is MyAttendanceViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
        }
    }

    private async void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MyAttendanceViewModel.SelectionChangeTick))
        {
            await AnimateSelectionCard();
        }
    }

    private async Task AnimateSelectionCard()
    {
        if (SelectionCard == null) return;

        await SelectionCard.ScaleTo(1.05, 80, Easing.CubicOut);
        await SelectionCard.ScaleTo(1.0, 120, Easing.CubicIn);
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