using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HSEM.Views
{
    public partial class WelcomePage : ContentPage
    {
        bool _started = false;

        public WelcomePage()
        {
            InitializeComponent();

            // الصفحة تظهر فارغة تمامًا مبدئياً
            // نجعل كل الصور مخفية (Opacity = 0) ونهيئ Transformات ابتدائية
            ImgFirst.Opacity = 0;
            ImgFirst.Scale = 0.2;

            ImgSecond.Opacity = 0;
            ImgSecond.TranslationX = 0;

            ImgThird.Opacity = 0;
            ImgThird.TranslationX = 0;

            ImgFourth.Opacity = 0;

            ImgFifth.Opacity = 0;
            ImgFifth.TranslationY = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_started) return;
            _started = true;

            await EnsureLayoutIsReady();
            var width = Math.Max(300, rootLayout.Width);
            var height = Math.Max(300, rootLayout.Height);

            // إعداد المواقع الابتدائية خارج الشاشة
            ImgSecond.TranslationX = width * 1.2;
            ImgThird.TranslationX = -width * 1.2;
            ImgFifth.TranslationY = -height * 0.9;

            try
            {
                // 1) أول صورة: Zoom in
                await AnimateFirstZoom();

                await Task.Delay(120); // فاصل صغير

                // 2) ثانية: slide from right
                await AnimateSlideInFromRight(ImgSecond);

                await Task.Delay(120);

                // 3) ثالثة: slide from left
                await AnimateSlideInFromLeft(ImgThird);

                await Task.Delay(120);

                // 4) رابعة: fade in
                await AnimateFadeIn(ImgFourth);

                await Task.Delay(100);

                // 5) خامسة: drop + bounce
                await AnimateDropBounce(ImgFifth);

                await Task.Delay(150);

                await NavigateToLoginPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Welcome animation error: {ex}");
            }
        }

        // 1) Zoom in for first image
        private async Task AnimateFirstZoom()
        {
            const uint zoomInDur = 550;   // نصف المدة السابقة
            const uint settleDur = 325;

            ImgFirst.Opacity = 1;

            await Task.WhenAll(
                ImgFirst.ScaleTo(1.18, zoomInDur, Easing.CubicOut),
                ImgFirst.FadeTo(1, zoomInDur, Easing.CubicInOut)
            );

            await ImgFirst.ScaleTo(0.96, 175, Easing.SinInOut);
            await ImgFirst.ScaleTo(1.0, settleDur, Easing.SpringOut);
        }

        // 2) Slide in from right
        private async Task AnimateSlideInFromRight(View view)
        {
            const uint dur = 425; // نصف الوقت
            view.Opacity = 1;

            await Task.WhenAll(
                view.TranslateTo(0, 0, dur, Easing.CubicOut),
                view.FadeTo(1, dur, Easing.CubicInOut)
            );
        }

        // 3) Slide in from left
        private async Task AnimateSlideInFromLeft(View view)
        {
            const uint dur = 425;
            view.Opacity = 1;

            await Task.WhenAll(
                view.TranslateTo(0, 0, dur, Easing.CubicOut),
                view.FadeTo(1, dur, Easing.CubicInOut)
            );
        }

        // 4) Fade in
        private async Task AnimateFadeIn(View view)
        {
            const uint dur = 450;
            view.Opacity = 0;
            await view.FadeTo(1, dur, Easing.SinInOut);
        }

        // 5) Drop + bounce
        private async Task AnimateDropBounce(View view)
        {
            view.Opacity = 1;
            double dropOffset = Math.Min(70, rootLayout.Height * 0.1);

            await view.TranslateTo(0, dropOffset, 275, Easing.CubicIn);
            await view.TranslateTo(0, -9, 130, Easing.CubicOut); // نصف الارتداد
            await view.TranslateTo(0, 0, 160, Easing.SinOut);    // العودة لمكانه
        }

        private async Task EnsureLayoutIsReady()
        {
            // ننتظر حتى يكون لـ rootLayout عرض وارتفاع مناسب
            int tries = 0;
            while ((rootLayout.Width <= 0 || rootLayout.Height <= 0) && tries < 40)
            {
                await Task.Delay(30);
                tries++;
            }
        }
        // Optional: navigate to main page (implement as you need)
        private async Task NavigateToLoginPage()
        {
            if (Application.Current.MainPage is FlyoutPage flyout &&
                flyout.Detail is NavigationPage nav)
            {
                var login = new LoginPage { Opacity = 0 };

                await nav.PushAsync(login, false);

                await Task.WhenAll(
                    this.FadeTo(0, 350, Easing.CubicInOut),
                    login.FadeTo(1, 350, Easing.CubicInOut)
                );

                nav.Navigation.RemovePage(this);
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        private async Task NavigateToMainPage()
        {
            // Example if app uses FlyoutPage and NavigationPage as in your earlier code:
            if (Application.Current.MainPage is FlyoutPage flyout &&
                flyout.Detail is NavigationPage nav)
            {
                var main = new AppFlyoutPage(); // or new MainPage()
                await nav.PushAsync(main, false);
                nav.Navigation.RemovePage(this);
            }
            else
            {
                // fallback: push Modal or set MainPage
                // Application.Current.MainPage = new AppFlyoutPage();
            }
        }
    }
}
