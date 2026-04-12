using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.ViewModels
{
    public class PopupViewModel : INotifyPropertyChanged
    {
        private string _popupTitle;
        public string PopupTitle
        {
            get => _popupTitle;
            set { _popupTitle = value; OnPropertyChanged(); }
        }

        private string _popupMessage;
        public string PopupMessage
        {
            get => _popupMessage;
            set { _popupMessage = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

}
