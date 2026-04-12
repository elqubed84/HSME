using HSEM.Interfaces;
using HSEM.Views;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class NavigationService : INavigationService
    {
        public async Task GoToAttendanceDetailsAsync(int year, int month)
        {
            var page = new MyAttendanceDetails(year, month);

            if (App.Current?.MainPage is FlyoutPage flyout &&
                flyout.Detail is NavigationPage nav)
            {
                await nav.Navigation.PushAsync(page);
                flyout.IsPresented = false; // إغلاق الـ menu
            }
        }

        public async Task GoToPayrollDetailsAsync(int year, int month, string monthName)
        {
            var page = new MySallaryDetails(year, month, monthName);

            if (App.Current?.MainPage is FlyoutPage flyout &&
                flyout.Detail is NavigationPage nav)
            {
                await nav.Navigation.PushAsync(page);
                flyout.IsPresented = false; // إغلاق الـ menu
            }
        }

        // لو حبيت تضيف navigation عام لأي صفحة
        public async Task NavigateToPageAsync(Page page)
        {
            if (App.Current?.MainPage is FlyoutPage flyout &&
                flyout.Detail is NavigationPage nav)
            {
                await nav.Navigation.PushAsync(page);
                flyout.IsPresented = false;
            }
        }
    }
}