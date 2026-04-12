using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controls.UserDialogs.Maui;
using global::Controls.UserDialogs.Maui;
using HSEM.Interfaces;
using Microsoft.Maui.Networking;
namespace HSEM.Services
{
    public class NetworkService : INetworkService
    {
        private readonly IUserDialogs _dialogs;
        private bool _wasDisconnected = false;

        public NetworkService(IUserDialogs dialogs)
        {
            _dialogs = dialogs;
        }

        public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        public void StartListening()
        {
            Connectivity.Current.ConnectivityChanged += Connectivity_ConnectivityChanged;

            // تحقق مبدئي
            if (!IsConnected)
            {
                ShowDisconnectedAlert();
            }
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet)
            {
                if (!_wasDisconnected)
                {
                    _wasDisconnected = true;
                    ShowDisconnectedAlert();
                }
            }
            else
            {
                if (_wasDisconnected)
                {
                    _wasDisconnected = false;
                    _dialogs.ShowToast("تم استعادة الاتصال بالسيرفر");
                }
            }
        }

        private void ShowDisconnectedAlert()
        {
            _dialogs.Alert("لا يوجد اتصال بالإنترنت. يرجى التحقق من الشبكة.", "تنبيه", "حسناً");
        }
    }

}
