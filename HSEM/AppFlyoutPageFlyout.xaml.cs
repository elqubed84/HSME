using HSEM.Models;
using HSEM.ViewModels;
using HSEM.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace HSEM;

public partial class AppFlyoutPageFlyout : ContentPage
{
    public ListView ListView;
    private AppFlyoutPageFlyoutViewModel _viewModel;
    public event PropertyChangedEventHandler PropertyChanged;

    public AppFlyoutPageFlyout()
    {
        InitializeComponent();
        _viewModel = new AppFlyoutPageFlyoutViewModel();
        BindingContext = _viewModel;
        ListView = MenuItemsListView;
        ListView.ItemSelected += async (sender, e) =>
        {
            if (e.SelectedItem is AppFlyoutPageFlyoutMenuItem item && item.TargetType != null)
            {
                if (Application.Current.MainPage is FlyoutPage fp)
                    fp.IsPresented = false;

                Page page = (Page)Activator.CreateInstance(item.TargetType);

                if (Application.Current.MainPage is FlyoutPage mainFlyout)
                {
                    if (mainFlyout.Detail is NavigationPage navPage)
                        await navPage.PushAsync(page);
                    else
                        mainFlyout.Detail = new NavigationPage(page);
                }
            }

    ((ListView)sender).SelectedItem = null; // ← ده مهم عشان تقدر تضغط تاني
        };
        //ListView.ItemSelected += async (sender, e) =>
        //{
        //    if (e.SelectedItem is AppFlyoutPageFlyoutMenuItem item && item.TargetType != null)
        //    {
        //        // إغلاق الفلاي أوت
        //        if (Application.Current.MainPage is FlyoutPage fp)
        //            fp.IsPresented = false;

        //        // إنشاء الصفحة والتنقل
        //        Page page = (Page)Activator.CreateInstance(item.TargetType);

        //        if (Application.Current.MainPage is FlyoutPage mainFlyout)
        //        {
        //            if (mainFlyout.Detail is NavigationPage navPage)
        //                await navPage.PushAsync(page);
        //            else
        //                mainFlyout.Detail = new NavigationPage(page);
        //        }
        //    }

        //   // إزالة التحديد لإعادة الضغط
        //   ((ListView)sender).SelectedItem = null;
        //};
    }
    public class MenuGroup : List<AppFlyoutPageFlyoutMenuItem>
    {
        public string GroupName { get; set; }

        public MenuGroup(string groupName, IEnumerable<AppFlyoutPageFlyoutMenuItem> items)
            : base(items)
        {
            GroupName = groupName;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // الـ ViewModel
    // ─────────────────────────────────────────────────────────────────────────
    public class AppFlyoutPageFlyoutViewModel : INotifyPropertyChanged
    {
        // ── الـ Collection اللي بيتعمل عليها Binding في الـ ListView ──
        public ObservableCollection<MenuGroup> MenuGroups { get; set; }

        public AppFlyoutPageFlyoutViewModel()
        {
            MenuGroups = new ObservableCollection<MenuGroup>();
            BuildMenu();
        }

        // ─────────────────────────────────────────────────────────────────────
        // بناء المنيو — كل AddGroup هو قسم مستقل
        // ─────────────────────────────────────────────────────────────────────
        private void BuildMenu()
        {
            // ── الرئيسية ──
            AddGroup("الرئيسية", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(1, IconFont.Dashboard, "معلوماتى",  typeof(Dashboard))
            });

            // ── مرتباتى ──
            AddGroup("مرتباتى", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(2, IconFont.Dashboard, "المرتبات", typeof(MySallary))
            });

            // ── الإجازات ──
            AddGroup("الإجازات", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(3, IconFont.Sick,  "طلب إجازة",      typeof(Views.LeaveRequest)),
                MenuItem(4, IconFont.List,  "طلبات الإجازات", typeof(NyLeaveRequests))
            });

            // ── المأموريات ──
            AddGroup("المأموريات", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(5, IconFont.Sick, "طلب مأمورية", typeof(Views.AddMissionRequest)),
                MenuItem(6, IconFont.List, "مأمورياتى",   typeof(MyMissionRequest))
            });

            // ── السلف ──
            AddGroup("السلف", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(7, IconFont.Money, "طلب سلفة",  typeof(AdvanceRequest)),
                MenuItem(8, IconFont.List,  "جميع السلف", typeof(MyAdvances))
            });

            // ── الأذون ──
            AddGroup("الأذون", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(9,  IconFont.Request_page, "طلب إذن", typeof(Views.PermissionRequest)),
                MenuItem(10, IconFont.List,         "طلباتى",  typeof(MyPermissions))
            });

            // ── الحالة ──
            AddGroup("الحالة", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(11, IconFont.Star,     "تقييماتى",  typeof(MyEvaluations)),
                MenuItem(12, IconFont.Plus_one, "إضافة بدل", typeof(AddOverTime)),
                MenuItem(13, IconFont.Plus_one, "بدلاتى",    typeof(MyOvertime)),
                MenuItem(14, IconFont.Moving,   "مؤثراتى",   typeof(MyAttendance))
            });

            // ── الرسائل ──
            AddGroup("الرسائل", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(15, IconFont.Message, "إرسال رسالة", typeof(SendMessage)),
                MenuItem(16, IconFont.Message, "الرسائل",      typeof(MyMessages))
            });

            // ── العهد والجزاءات ──
            AddGroup("العهد والجزاءات", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(17, IconFont.Pin_end, "الجزاءات",  typeof(MyDisciplinary)),
                MenuItem(18, IconFont.Laptop,  "العهد",      typeof(MyAssets)),
                MenuItem(19, IconFont.Book,    "عقد العمل",  typeof(MyContract))
            });

            // ── الحساب ──
            AddGroup("الحساب", new List<AppFlyoutPageFlyoutMenuItem>
            {
                MenuItem(20, IconFont.Wifi,     "تغيير كلمة السر", typeof(ChangePassword)),
                MenuItem(21, IconFont.Password, "تسجيل الخروج",    typeof(SignOut))
            });

            // ── عناصر الأدمن تتضاف بعد التحقق من الـ Token ──
            _ = AddAdminGroupAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper — ينشئ MenuItem بأقل كود
        // ─────────────────────────────────────────────────────────────────────
        private static AppFlyoutPageFlyoutMenuItem MenuItem(
            int id, string glyph, string title, Type targetType)
        {
            return new AppFlyoutPageFlyoutMenuItem
            {
                Id = id,
                Title = title,
                TargetType = targetType,
                Icon = new FontImageSource
                {
                    FontFamily = "Icon",
                    Glyph = glyph,
                    Size = 22,
                    Color = Colors.White
                }
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper — يضيف Group للـ Collection
        // ─────────────────────────────────────────────────────────────────────
        private void AddGroup(string name, List<AppFlyoutPageFlyoutMenuItem> items)
        {
            MenuGroups.Add(new MenuGroup(name, items));
        }

        // ─────────────────────────────────────────────────────────────────────
        // التحقق من صلاحية الأدمن وإضافة قسم الإدارة
        // ─────────────────────────────────────────────────────────────────────
        private async Task AddAdminGroupAsync()
        {
            try
            {
                var accessToken = await SecureStorage.Default.GetAsync("AccessToken");

                if (string.IsNullOrEmpty(accessToken))
                    return;

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);

                bool isAdmin = jwt.Claims.Any(c =>
                    c.Type == ClaimTypes.Role && c.Value == "Admin");

                if (!isAdmin)
                    return;

                // نضيف قسم الإدارة على الـ Main Thread عشان نعدل الـ Collection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AddGroup("الإدارة", new List<AppFlyoutPageFlyoutMenuItem>
                    {
                        MenuItem(50, IconFont.Wifi,        "بصمة الشركة",           typeof(CompanyPrint)),
                        MenuItem(51, IconFont.Fingerprint, "بصمات أجهزة الموظفين",  typeof(DevicePrint))
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في التحقق من صلاحية الأدمن: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // INotifyPropertyChanged
        // ─────────────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    //private class AppFlyoutPageFlyoutViewModel
    //{
    //    public ObservableCollection<AppFlyoutPageFlyoutMenuItem> MenuItems { get; set; }

    //    public AppFlyoutPageFlyoutViewModel()
    //    {
    //        MenuItems = new ObservableCollection<AppFlyoutPageFlyoutMenuItem>();

    //        // عناصر عامة لكل المستخدمين
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Dashboard, Size = 22, Color = Colors.White }, Title = "معلوماتى", IsSeparatorVisible = true, Hed = "الرئيسية", TargetType = typeof(Dashboard) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 2, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Dashboard, Size = 22, Color = Colors.White }, Title = "المرتبات", IsSeparatorVisible = true, Hed = "مرتباتى", TargetType = typeof(MySallary) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 3, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Sick, Size = 22, Color = Colors.White }, Title = "طلب إجازة", IsSeparatorVisible = true, Hed = "الإجازات", TargetType = typeof(Views.LeaveRequest) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 4, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "طلبات الاجازات", IsSeparatorVisible = false, TargetType = typeof(NyLeaveRequests) });


    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 3, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Sick, Size = 22, Color = Colors.White }, Title = "طلب مأمورية", IsSeparatorVisible = true, Hed = "المأموريات", TargetType = typeof(Views.AddMissionRequest) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 4, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "مأمورياتى", IsSeparatorVisible = false, TargetType = typeof(MyMissionRequest) });







    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 5, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Money, Size = 22, Color = Colors.White }, Title = "طلب سلفة", IsSeparatorVisible = true, Hed = "السلف", TargetType = typeof(AdvanceRequest) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 6, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "جميع السلف", IsSeparatorVisible = false, TargetType = typeof(MyAdvances) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 7, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Request_page, Size = 22, Color = Colors.White }, Title = "طلب مأمورية", IsSeparatorVisible = true, Hed = "المأموريات", TargetType = typeof(Views.PermissionRequest) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 7, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Request_page, Size = 22, Color = Colors.White }, Title = "مأمورياتى", IsSeparatorVisible = false, TargetType = typeof(Views.PermissionRequest) });

    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 7, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Request_page, Size = 22, Color = Colors.White }, Title = "طلب إذن", IsSeparatorVisible = true, Hed = "الأوذون", TargetType = typeof(Views.PermissionRequest) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 8, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "طلباتى", IsSeparatorVisible = false, TargetType = typeof(MyPermissions) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 9, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Star, Size = 22, Color = Colors.White }, Title = "تقييماتى", IsSeparatorVisible = true, Hed = "الحالة", TargetType = typeof(MyEvaluations) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 10, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Plus_one, Size = 22, Color = Colors.White }, Title = "إضافة بدل", IsSeparatorVisible = false, TargetType = typeof(AddOverTime) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 11, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Plus_one, Size = 22, Color = Colors.White }, Title = "بدلاتى", IsSeparatorVisible = false, TargetType = typeof(MyOvertime) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 12, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Moving, Size = 22, Color = Colors.White }, Title = "مؤثراتى", IsSeparatorVisible = false, TargetType = typeof(MyAttendance) });


    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 3, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Message, Size = 22, Color = Colors.White }, Title = "إرسال رسالة", IsSeparatorVisible = true, Hed = "الرسائل", TargetType = typeof(SendMessage) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 4, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Message, Size = 22, Color = Colors.White }, Title = "الرسائل", IsSeparatorVisible = false, TargetType = typeof(MyMessages) });

    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 3, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Pin_end, Size = 22, Color = Colors.White }, Title = "الجزاءات", IsSeparatorVisible = true, Hed = "العهد والجزاءات", TargetType = typeof(MyDisciplinary) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 4, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Laptop, Size = 22, Color = Colors.White }, Title = "العهد", IsSeparatorVisible = false, TargetType = typeof(MyAssets) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 4, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Book, Size = 22, Color = Colors.White }, Title = "عقد العمل", IsSeparatorVisible = false, TargetType = typeof(MyContract) });






    //        // عناصر الحساب
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 13, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Wifi, Size = 22, Color = Colors.White }, Title = "تغيير كلمة السر", IsSeparatorVisible = true, Hed = "الحساب", TargetType = typeof(ChangePassword) });
    //        MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 14, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Password, Size = 22, Color = Colors.White }, Title = "تسجيل الخروج", IsSeparatorVisible = false, TargetType = typeof(SignOut) });

    //        // تحقق من صلاحية الادمن لإضافة السطرين الخاصين بالادمن
    //        AddAdminItemsIfAdminAsync().ConfigureAwait(false);
    //    }

    //    private async Task AddAdminItemsIfAdminAsync()
    //    {
    //        try
    //        {
    //            var accessToken = await SecureStorage.Default.GetAsync("AccessToken");
    //            if (!string.IsNullOrEmpty(accessToken))
    //            {
    //                var handler = new JwtSecurityTokenHandler();
    //                var jwt = handler.ReadJwtToken(accessToken);

    //                bool isAdmin = jwt.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

    //                if (isAdmin)
    //                {
    //                    MenuItems.Add(new AppFlyoutPageFlyoutMenuItem
    //                    {
    //                        Id = 15,
    //                        Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Wifi, Size = 22, Color = Colors.White },
    //                        Title = "بصمة الشركة",
    //                        IsSeparatorVisible = true,
    //                        Hed = "الإدارة",
    //                        TargetType = typeof(CompanyPrint)
    //                    });

    //                    MenuItems.Add(new AppFlyoutPageFlyoutMenuItem
    //                    {
    //                        Id = 16,
    //                        Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Fingerprint, Size = 22, Color = Colors.White },
    //                        Title = "بصمات أجهزة الموظفين",
    //                        IsSeparatorVisible = false,
    //                        TargetType = typeof(DevicePrint)
    //                    });
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"❌ خطأ في التحقق من صلاحية الادمن: {ex.Message}");
    //        }
    //    }

    //    #region INotifyPropertyChanged Implementation
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    void OnPropertyChanged([CallerMemberName] string propertyName = "")
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //}
}
