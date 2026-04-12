//using HSEM.Models;
//using HSEM.Views;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace HSEM.ViewModels
//{
//    public class AppFlyoutPageFlyoutViewModel : INotifyPropertyChanged
//    {
//        public ObservableCollection<AppFlyoutPageFlyoutMenuItem> MenuItems { get; set; }

//        public AppFlyoutPageFlyoutViewModel()
//        {
//            MenuItems = new ObservableCollection<AppFlyoutPageFlyoutMenuItem>();

//            // العناصر الأساسية
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Dashboard, Size = 22, Color = Colors.White }, Title = "معلوماتى", IsSeparatorVisible = true, Hed = "الرئيسية", TargetType = typeof(Dashboard) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Dashboard, Size = 22, Color = Colors.White }, Title = "المرتبات", IsSeparatorVisible = true, Hed = "مرتباتى", TargetType = typeof(MySallary) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Sick, Size = 22, Color = Colors.White }, Title = "طلب إجازة", IsSeparatorVisible = true, Hed = "الإجازات", TargetType = typeof(Views.LeaveRequest) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "طلبات الاجازات", IsSeparatorVisible = false, TargetType = typeof(NyLeaveRequests) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Money, Size = 22, Color = Colors.White }, Title = "طلب سلفة", IsSeparatorVisible = true, Hed = "السلف", TargetType = typeof(AdvanceRequest) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "جميع السلف", IsSeparatorVisible = false, TargetType = typeof(MyAdvances) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Request_page, Size = 22, Color = Colors.White }, Title = "طلب إذن", IsSeparatorVisible = true, Hed = "الأوذون", TargetType = typeof(Views.PermissionRequest) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.List, Size = 22, Color = Colors.White }, Title = "طلباتى", IsSeparatorVisible = false, TargetType = typeof(MyPermissions) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Star, Size = 22, Color = Colors.White }, Title = "تقييماتى", IsSeparatorVisible = true, Hed = "الحالة", TargetType = typeof(MyEvaluations) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Plus_one, Size = 22, Color = Colors.White }, Title = "إضافة بدل", IsSeparatorVisible = false, TargetType = typeof(AddOverTime) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Plus_one, Size = 22, Color = Colors.White }, Title = "بدلاتى", IsSeparatorVisible = false, TargetType = typeof(MyOvertime) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Moving, Size = 22, Color = Colors.White }, Title = "مؤثراتى", IsSeparatorVisible = false, TargetType = typeof(MyAttendance) });
//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem { Id = 1, Icon = new FontImageSource { FontFamily = "Icon", Glyph = IconFont.Dashboard, Size = 22, Color = Colors.White }, Title = "تسجيل الخروج", IsSeparatorVisible = true, Hed = "الحساب", TargetType = typeof(SignOut) });

//            // باقي العناصر العادية...
//        }

//        public async Task InitializeAsync()
//        {
//            var accessToken = await SecureStorage.Default.GetAsync("AccessToken");

//            if (!string.IsNullOrEmpty(accessToken))
//            {
//                var handler = new JwtSecurityTokenHandler();
//                var jwt = handler.ReadJwtToken(accessToken);

//                bool isAdmin = jwt.Claims.Any(c =>
//                    c.Type == ClaimTypes.Role && c.Value == "Admin");

//                if (isAdmin)
//                {
//                    MenuItems.Add(new AppFlyoutPageFlyoutMenuItem
//                    {
//                        Title = "بصمة الشركة",
//                        TargetType = typeof(CompanyPrint)
//                    });

//                    MenuItems.Add(new AppFlyoutPageFlyoutMenuItem
//                    {
//                        Title = "بصمة الأجهزة",
//                        TargetType = typeof(DevicePrint)
//                    });
//                }
//            }

//            MenuItems.Add(new AppFlyoutPageFlyoutMenuItem
//            {
//                Title = "تسجيل الخروج",
//                TargetType = typeof(SignOut)
//            });
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//    }

//}
