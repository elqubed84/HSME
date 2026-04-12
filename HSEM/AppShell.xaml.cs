using HSEM.Views;

namespace HSEM
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // تسجيل كل الروتز
            RegisterRoutes();
        }

        void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(Dashboard), typeof(Dashboard));
            Routing.RegisterRoute(nameof(LeaveRequest), typeof(LeaveRequest));
            Routing.RegisterRoute(nameof(NyLeaveRequests), typeof(NyLeaveRequests));
            Routing.RegisterRoute(nameof(AdvanceRequest), typeof(AdvanceRequest));
            Routing.RegisterRoute(nameof(MyAdvances), typeof(MyAdvances));
            Routing.RegisterRoute(nameof(MyAttendance), typeof(MyAttendance));
            Routing.RegisterRoute(nameof(MyOvertime), typeof(MyOvertime));
            Routing.RegisterRoute(nameof(PermissionRequest), typeof(PermissionRequest));
            Routing.RegisterRoute(nameof(MyEvaluations), typeof(MyEvaluations));
        }
    }
}
