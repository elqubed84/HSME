using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HSEM.Views
{
    public partial class WlecomPage : ContentPage
    {
        bool started = false;

        public WlecomPage()
        {
            InitializeComponent();

            ImgAA.Opacity = 0;
            ImgBB.Opacity = 0;
            ImgCC.Opacity = 0;
            ImgDD.Opacity = 0;
            ImgEE.Opacity = 0;

            ImgEE.Scale = 0.1;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (started) return;
            started = true;

            await WaitLayout();

            double width = rootLayout.Width;
            double height = rootLayout.Height;

            ImgAA.TranslationX = width;
            ImgBB.TranslationY = -height;
            ImgCC.TranslationY = height;
            ImgDD.TranslationX = -width;

            try
            {
                // دخول الأجزاء بدون Delay
                await AnimateRight(ImgAA);
                await AnimateTop(ImgBB);
                await AnimateBottom(ImgCC);
                await AnimateLeft(ImgDD);

                // الجزء الأخير
                await AnimateZoom(ImgEE);

                await FinalEffect();

                // انتظار بسيط لرؤية اللوجو
                await Task.Delay(800);

                await NavigateToLoginPage();
            }
            catch (Exception exception)
            {
                await DisplayAlert("خطأ", exception.Message, "تمام");
            }
        }

        async Task AnimateRight(View v)
        {
            v.Opacity = 1;
            await v.TranslateTo(0, 0, 450, Easing.CubicOut);
        }

        async Task AnimateTop(View v)
        {
            v.Opacity = 1;
            await v.TranslateTo(0, 0, 450, Easing.CubicOut);
        }

        async Task AnimateBottom(View v)
        {
            v.Opacity = 1;
            await v.TranslateTo(0, 0, 450, Easing.CubicOut);
        }

        async Task AnimateLeft(View v)
        {
            v.Opacity = 1;
            await v.TranslateTo(0, 0, 450, Easing.CubicOut);
        }

        async Task AnimateZoom(View v)
        {
            v.Opacity = 1;

            await v.ScaleTo(1.2, 350, Easing.CubicOut);
            await v.ScaleTo(1, 220, Easing.SpringOut);
        }

        async Task FinalEffect()
        {
            await rootLayout.ScaleTo(1.05, 200, Easing.CubicOut);
            await rootLayout.ScaleTo(1, 200, Easing.CubicInOut);
        }

        async Task WaitLayout()
        {
            int i = 0;

            while ((rootLayout.Width <= 0 || rootLayout.Height <= 0) && i < 40)
            {
                await Task.Delay(30);
                i++;
            }
        }

        // الانتقال للـ Login
        private async Task NavigateToLoginPage()
        {
            if (Application.Current.MainPage is FlyoutPage flyout)
            {
                flyout.Detail = new NavigationPage(new LoginPage());
            }
            else
            {
                // fallback لو مكنش FlyoutPage
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

    }
}