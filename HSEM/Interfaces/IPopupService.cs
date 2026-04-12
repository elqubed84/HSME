using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IPopupService
    {
      
        Task<bool> ShowAlertAsyncbool(
            string title,
            string message,
            string acceptText);
        Task ShowAlertAsync(string title, string message, string confirmText);
        void ShowSuccessToast(string message);
        Task<bool> ShowConfirmAsync(string title, string message,
            string acceptText = "نعم", string cancelText = "لا");
    }

}
