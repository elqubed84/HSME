using HSEM.ViewModels;
using HSEM.Views;

namespace HSEM;

public partial class AppFlyoutPage : FlyoutPage
{
    public AppFlyoutPageFlyout FlyoutPageFlyout => Flyout as AppFlyoutPageFlyout;
    public AppFlyoutPage()
    {
        InitializeComponent();
        Flyout = new AppFlyoutPageFlyout();
        
        BindingContext = new BaseViewModel();
    }

    protected override void OnAppearing()
{
    base.OnAppearing();
    // التأكد من ربط الـ Event بالنسخة الموجودة فعلياً في الـ XAML
    if (FlyoutPage?.ListView != null)
    {
        FlyoutPage.ListView.ItemSelected -= ListView_ItemSelected;
        FlyoutPage.ListView.ItemSelected += ListView_ItemSelected;
    }
}

    private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is AppFlyoutPageFlyoutMenuItem item)
        {
            var page = (Page)Activator.CreateInstance(item.TargetType);
            var nav = Detail as NavigationPage;

            if (nav != null)
                await nav.PushAsync(page);

            FlyoutPage.ListView.SelectedItem = null;
            IsPresented = false;
        }
    }
}
