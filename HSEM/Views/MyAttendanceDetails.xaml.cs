using HSEM.Helper;
using HSEM.ViewModels;

namespace HSEM.Views;

public partial class MyAttendanceDetails : ContentPage
{
   

    //public int Year { get; }
    //public int Month { get; }
    public MyAttendanceDetails(int year,int month)
	{
		InitializeComponent();
        var vm = new MyAttendanceDetailsViewModel();
        BindingContext = vm;

        vm.Year = year;
        vm.Month = month;

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MyAttendanceDetailsViewModel vm)
        {
            try
            {
                // أظهر اللودر فورًا (نضمن ظهور الواجهة أولاً)
                Overlay.IsVisible = true;        // grid overlay
                                                 // لو في حاجة اسمها Loader (BusyIndicator) يمكنك ضبطها مباشرة أيضاً:
                                                 // Loader.IsVisible = true; Loader.IsRunning = true;

                // انتظر دورة רינדرة قصيرة ليعطي الـ UI فرصة للرسم
                await Task.Yield();

                // ثم حمّل الداتا
                await vm.LoadAsync();
            }
            finally
            {
                // أخفِ اللودر بعد الانتهاء
                Overlay.IsVisible = false;
                // Loader.IsRunning = false; Loader.IsVisible = false;
            }
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