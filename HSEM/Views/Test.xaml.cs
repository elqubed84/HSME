
using HSEM.ViewModels;
using Syncfusion.Maui.DataGrid;
using System.Collections.ObjectModel;

namespace HSEM.Views;

public partial class Test : ContentPage
{
    public Test()
    {
        InitializeComponent();
    }
    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (BindingContext is YearWheelViewModel vm)
        {
            int chosenYear = vm.SelectedYear;
            // مثال: تنقل لصفحة تانية مع السنة المختارة
            await Navigation.PushAsync(new MyAttendanceDetails(chosenYear, DateTime.Now.Month));
            // أو ترجع النتيجة عبر MessagingCenter أو Callback حسب تصميمك
        }
    }
}
