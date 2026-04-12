using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace HSEM
{
    internal class MyNotificationPermission : BasePlatformPermission
    {
#if ANDROID
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string permission, bool isRuntime)>
            {
            ("android.permission.POST_NOTIFICATIONS", true),
                // ("android.permission.USE_EXACT_ALARM", true)
                // ("android.permission.SCHEDULE_EXACT_ALARM", true)
            }.ToArray();
#endif
    }
}
