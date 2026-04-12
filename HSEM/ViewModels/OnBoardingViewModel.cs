using System.Windows.Input;

namespace HSEM
{
    public class OnboardingViewModel : BindableObject
    {
        public List<OnBoardingModel> OnboardingItems { get; }
        
        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _dontShowAgain;
        public bool DontShowAgain
        {
            get => _dontShowAgain;
            set
            {
                _dontShowAgain = value;
                OnPropertyChanged();
            }
        }
        public ICommand FinishCommand { get; }
        public OnboardingViewModel()
        {
            OnboardingItems = new List<OnBoardingModel>
{
    new OnBoardingModel(
        "onboardingimage1.png",
        "إدارة الموظفين بسهولة",
        "تابع بيانات الموظفين، الوظائف، والتفاصيل من مكان واحد بكل سهولة."
    ),

    new OnBoardingModel(
        "onboardingimage2.png",
        "الحضور والانصراف الذكي",
        "سجل حضورك وانصرافك بسهولة مع نظام دقيق وتقارير فورية."
    ),

    new OnBoardingModel(
        "onboardingimage3.png",
        "طلبات الإجازات والتقارير",
        "قدم طلبات الإجازة وتابع حالتها واحصل على تقارير دقيقة في أي وقت."
    )
};
            FinishCommand = new Command(SavePreference);
        }
        private void SavePreference()
        {
            Preferences.Set("SkipOnboarding", DontShowAgain);
        }
    }

    public class OnBoardingModel
    {
        public OnBoardingModel(string image, string title, string description)
        {
            Image =image;
            Title = title;
            Description = description;
        }

        private string? image;
        private string? title;
        private string? description;

        public string? Image
        {
            get { return image; }
            set { image = value; }
        }

        public string? Title
        {
            get { return title; }
            set { title = value; }
        }

        public string? Description
        {
            get { return description; }
            set { description = value; }
        }
       
    }
}
