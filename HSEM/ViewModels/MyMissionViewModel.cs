using HSEM.Models;
using HSEM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MyMissionViewModel : BindableObject
    {
        private readonly MissionService _service;
        public ObservableCollection<Models.MissionDto> MyMissions { get; set; }
            = new ObservableCollection<Models.MissionDto>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }

        public MyMissionViewModel()
        {
            _service = new MissionService();
            LoadCommand = new Command(async () => await LoadDataAsync());

            // ✅ استخدم Task.Run بـ try-catch
            _ = SafeLoadAsync();
        }

        private async Task SafeLoadAsync()
        {
            try
            {
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load error: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    var list = await _service.GetMyMissionsAsync(token);
                    MyMissions.Clear();
                    foreach (var item in list)
                        MyMissions.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadData error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
