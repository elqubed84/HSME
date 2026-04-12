using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class SendMessage : ContentPage
{
	public SendMessage()
	{
		InitializeComponent();
	}

    private void OnEmployeeSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as EmployeeDto;
        if (selected == null) return;

        SearchEntry.Text = $"{selected.fullName} ({selected.employeeCode})";

        if (BindingContext is SendMessageViewModel vm)
        {
            vm.SelectedEmployee = selected;
            vm.EmployeeSelected(); // اخفاء الاقتراحات بعد الاختيار
        }

     ((CollectionView)sender).SelectedItem = null;
    }

    private void OnEmployeeTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var selected = frame?.BindingContext as EmployeeDto;
        if (selected == null) return;

        SearchEntry.Text = $"{selected.fullName} ({selected.employeeCode})";

        if (BindingContext is SendMessageViewModel vm)
        {
            vm.SelectedEmployee = selected;
            vm.EmployeeSelected(); // اخفاء الاقتراحات بعد الاختيار
        }
    }
    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is SendMessageViewModel vm)
        {
            vm.SearchCommand.Execute(e.NewTextValue);
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