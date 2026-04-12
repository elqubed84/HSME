//using Acr.UserDialogs;
using Syncfusion.Maui.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        bool isLoading = false;
        bool isShow = false;
		SfAvatarView imagelogo = null;
		bool isInable = true;
        bool isOpen = false;
		bool isOpen2 = false;
		bool openloader = false;
		string Message = string.Empty;
        string flyoutMenu = "Default";
        string TitleMessage = string.Empty;
		Color ColorMessage = Color.FromRgb(0, 0, 0);

        string IconStatus = string.Empty;
        public string FlyoutMenu
        {
            get { return flyoutMenu; }
            set { SetProperty(ref flyoutMenu, value); }
        }
        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }
		public bool IsShow
		{
			get { return isShow; }
			set { SetProperty(ref isShow, value); }
		}
		public bool IsInable
		{
			get { return isInable; }
			set { SetProperty(ref isInable, value); }
		}
		public bool IsOpen
		{
			get { return isOpen; }
			set { SetProperty(ref isOpen, value); }
		}
		public bool IsOpen2
		{
			get { return isOpen2; }
			set { SetProperty(ref isOpen2, value); }
		}
		public bool Openloader
		{
			get { return openloader; }
			set { SetProperty(ref openloader, value); }
		}

		string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }
		string notiHeader = string.Empty;
		public string NotiHeader
		{
			get { return notiHeader; }
			set { SetProperty(ref notiHeader, value); }
		}
		string notiMessage = string.Empty;
		public string NotiMessage
		{
			get { return notiMessage; }
			set { SetProperty(ref notiMessage, value); }
		}
		string imageNoti = string.Empty;
		public string ImageNoti
		{
			get { return imageNoti; }
			set { SetProperty(ref imageNoti, value); }
		}
		public SfAvatarView Imagelogo
		{
			get { return imagelogo; }
			set { SetProperty(ref imagelogo, value); }
		}
		public string message
		{
			get { return Message; }
			set { SetProperty(ref Message, value); }
		}
		public string titleMessage
		{
			get { return TitleMessage; }
			set { SetProperty(ref TitleMessage, value); }
		}
		public Color colorMessage
		{
			get { return ColorMessage; }
			set { SetProperty(ref ColorMessage, value); }
		}
		public string iconStatus
		{
			get { return IconStatus; }
			set { SetProperty(ref IconStatus, value); }
		}
		private bool isSelected;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				OnPropertyChanged("IsSelected");
			}
		}
		protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
		

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
