using Controls.UserDialogs.Maui;
using HSEM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class AlertService : IAlertService
    {
        private readonly IUserDialogs _dialogs;
        public AlertService(IUserDialogs dialogs)
        {
            _dialogs = dialogs;
        }
        public Task ShowAsync(string title, string message, string cancel)
        {
            _dialogs.Alert(message, title, cancel);
            return Task.CompletedTask;
            //return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }
}
