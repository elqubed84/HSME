using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class EmployeeEvaluationDetailsPage : ContentPage
{
    private readonly EmployeeEvaluationDetailsViewModel _vm;

    public EmployeeEvaluationDetailsPage(int month, int year)
    {
        InitializeComponent();

        _vm = new EmployeeEvaluationDetailsViewModel(month, year);
        BindingContext = _vm;

        //// تشغيل التحميل بعد الـ BindingContext
        //Loaded += async (s, e) => await _vm.LoadAsync();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
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