using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Helper
{
    public static class NavigationHelper
    {
        public static bool HandleBackButton()
        {
            if (App.Current?.MainPage is FlyoutPage masterDetail &&
                masterDetail.Detail is NavigationPage navigationPage)
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (navigationPage.Navigation.NavigationStack.Count > 1)
                    {
                        await navigationPage.PopAsync();
                    }
                });
                return true;
            }
            return false; // خلي النظام يتعامل مع الـ back
        }

        public static async Task PopCurrentPageAsync()
        {
            if (App.Current?.MainPage is FlyoutPage masterDetail &&
                masterDetail.Detail is NavigationPage navigationPage)
            {
                if (navigationPage.Navigation.NavigationStack.Count > 1)
                {
                    await navigationPage.PopAsync();
                }
            }
        }

        public static void ToggleFlyoutMenu()
        {
            if (App.Current?.MainPage is FlyoutPage masterDetail)
            {
                masterDetail.IsPresented = !masterDetail.IsPresented;
            }
        }
    }

}
