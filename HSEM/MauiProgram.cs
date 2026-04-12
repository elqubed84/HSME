using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.CloudMessaging;
using Controls.UserDialogs.Maui;
using HSEM.Interfaces;

using HSEM.Services;
using Plugin.Maui.Biometric;
using Syncfusion.Maui.Toolkit.Hosting;





#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif
namespace HSEM
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
               // .ConfigureSyncfusionToolkit()
                .UseMauiCommunityToolkit()
                 .ConfigureSyncfusionToolkit()
                .RegisterFirebaseServices()
                .ConfigureSyncfusionCore()
                .UseUserDialogs(() =>
                {
                    //setup your default styles for dialogs
                    // لون الخلفية الجديد
                    AlertConfig.DefaultBackgroundColor = Colors.LightGray;
                    // لون النص الجديد
                    AlertConfig.DefaultMessageColor = Color.FromArgb("#174da1");
#if ANDROID
                    AlertConfig.DefaultMessageFontFamily = "Montserrat-Medium.ttf";
                    AlertConfig.DefaultTitleFontFamily = "Montserrat-Medium.ttf";
                    AlertConfig.DefaultPositiveButtonFontFamily = "Montserrat-Medium.ttf";
                    AlertConfig.DefaultMessageFontSize = 20;
                    AlertConfig.DefaultPositiveButtonTextColor = Color.FromArgb("#174da1");
                    AlertConfig.DefaultTitleColor = Color.FromArgb("#174da1");
#else
        AlertConfig.DefaultMessageFontFamily = "Montserrat-Medium.ttf";
#endif

                    ToastConfig.DefaultCornerRadius = 15;
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Montserrat-Regular.ttf", "Montserrat-Regular");
                    fonts.AddFont("Montserrat-SemiBold.ttf", "Montserrat-SemiBold");
                    fonts.AddFont("Montserrat-Medium.ttf", "Montserrat-Medium");
                    fonts.AddFont("Montserrat-Bold.ttf", "Montserrat-Bold");
                    fonts.AddFont("PopupFontIcons.ttf", "PopupFontIcons");
                    fonts.AddFont("UIFontIcons.ttf", "FontIcons");
                    fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined-Regular");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "Icon");
                });
            //Register Syncfusion license https://help.syncfusion.com/common/essential-studio/licensing/how-to-generate
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1cdnRdR2leVEFyXkVWYEs=");

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {

            builder.ConfigureLifecycleEvents(events => {
#if IOS
        events.AddiOS(iOS => iOS.WillFinishLaunching((_, __) => {
            CrossFirebase.Initialize();
            FirebaseCloudMessagingImplementation.Initialize();
            return false;
        }));
#elif ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity)));
#endif
            });

            builder.Services.AddSingleton<INetworkService, NetworkService>();
            builder.Services.AddSingleton<IPopupService, PopupService>();
            builder.Services.AddSingleton<IBiometric>(BiometricAuthenticationService.Default);
            return builder;
        }
    }
}
