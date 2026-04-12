using Controls.UserDialogs.Maui;
using HSEM.Interfaces;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HSEM.Services
{
    public class PopupService : IPopupService
    {
        private readonly IUserDialogs _dialogs;

        public PopupService()
        {
            // إنشاء instance مباشرة
            _dialogs = UserDialogs.Instance;
        }
        public Task<bool> ShowAlertAsyncbool(string title, string message, string acceptText)
        {
            _dialogs.Alert(message, title, acceptText);
            return _dialogs.ConfirmAsync(new ConfirmConfig
            {
                Title = title,
                Message = message,
                OkText = acceptText
            });
        }
        public Task ShowAlertAsync(string title, string message, string confirmText)
        {
            _dialogs.Alert(message, title, confirmText);
            return Task.CompletedTask;
        }

        // ✅ جديد — للنجاح السريع (مش بيوقف المستخدم)
        public void ShowSuccessToast(string message)
        {
            _dialogs.ShowToast(message, TimeSpan.FromSeconds(2).ToString());

        }

        // ✅ جديد — للتأكيد قبل عمل حاجة خطيرة
        public Task<bool> ShowConfirmAsync(string title, string message,
            string acceptText = "نعم", string cancelText = "لا")
        {
            return _dialogs.ConfirmAsync(new ConfirmConfig
            {
                Title = title,
                Message = message,
                OkText = acceptText,
                CancelText = cancelText
            });
        }
    }
}

