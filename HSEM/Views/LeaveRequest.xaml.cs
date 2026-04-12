using HSEM.Helper;
using HSEM.Interfaces;
using HSEM.Services;
using Microsoft.Maui.Storage;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace HSEM.Views;

public partial class LeaveRequest : ContentPage
{
    private FileResult? selectedFile;
    private readonly IPopupService _alertService;
    public LeaveRequest()
    {
        InitializeComponent();
        _alertService = new PopupService();
        // تعبئة نوع الإجازة
        LeaveTypePicker.ItemsSource = Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>().ToList();
    }

    private async void OnSelectAttachmentClicked(object sender, EventArgs e)
    {
        try
        {
            selectedFile = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "اختر المرفق"
            });

            if (selectedFile != null)
                AttachmentEntry.Text = selectedFile.FileName;
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", ex.Message, "موافق");
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (LeaveTypePicker.SelectedItem == null)
        {
            await _alertService.ShowAlertAsync("خطأ", "اختر نوع الإجازة", "موافق");
            return;
        }

        if (StartDatePicker.Date > EndDatePicker.Date)
        {
            await _alertService.ShowAlertAsync("خطأ", "تاريخ النهاية يجب أن يكون بعد البداية", "موافق");
            return;
        }
        var selectedLeaveType = (LeaveType)LeaveTypePicker.SelectedItem;

        // 🔴 شرط الإجازة المرضي
        if ((selectedLeaveType == LeaveType.Sick ||
     selectedLeaveType == LeaveType.Hajj ||
     selectedLeaveType == LeaveType.Umrah ||
     selectedLeaveType == LeaveType.Wwladah ||
     selectedLeaveType == LeaveType.Marriage) && selectedFile == null)
        {
            await _alertService.ShowAlertAsync("تنبيه", "يجب إرفاق مستند لهذا النوع من الإجازة", "موافق");
            return;
        }
        Loader.IsVisible = true;

        try
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(LeaveTypePicker.SelectedItem.ToString()), "LeaveType");
            content.Add(new StringContent(StartDatePicker.Date.ToString("yyyy-MM-dd")), "StartDate");
            content.Add(new StringContent(EndDatePicker.Date.ToString("yyyy-MM-dd")), "EndDate");

            if (selectedFile != null)
            {
                var stream = await selectedFile.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png"); // حسب نوع الصورة
                content.Add(fileContent, "Attachment", selectedFile.FileName);
            }

            var response = await LoginPage.PostWithTokenAsync("https://elnagarygroup-001-site1.ktempurl.com/api/LeaveApi/AddLeave", content);
            var resultString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultString);
            var result = doc.RootElement;

            if (response.IsSuccessStatusCode)
            {
                await _alertService.ShowAlertAsync("نجاح 🎉", result.GetProperty("message").GetString(), "تمام");
                await Navigation.PushAsync(new Dashboard());

            }
            else
            {
                await _alertService.ShowAlertAsync("خطأ 😅", result.GetProperty("message").GetString(), "موافق");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", ex.Message, "موافق");
        }
        finally
        {
            Loader.IsVisible = false;
        }
    }

    private async Task<string> FileToBase64(FileResult file)
    {
        using var stream = await file.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return Convert.ToBase64String(ms.ToArray());
    }
    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await NavigationHelper.PopCurrentPageAsync();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        NavigationHelper.ToggleFlyoutMenu();
    }

    protected override bool OnBackButtonPressed()
    {
        return NavigationHelper.HandleBackButton();
    }
}

// نموذج enum لتوضيح نوع الإجازة
public enum LeaveType
{
    [Display(Name = "عارضة")]
    Casual,

    [Display(Name = "اعتيادية")]
    Annual,

    [Display(Name = "مرضية")]
    Sick,

    [Display(Name = "إجازة زواج")]
    Marriage,

    [Display(Name = "إجازة حالة وفاة")]
    Death,

    [Display(Name = "إجازة الحج")]
    Hajj,

    [Display(Name = "إجازة العمرة")]
    Umrah,

    [Display(Name = "إجازة الوضع")]
    Wwladah,

    [Display(Name = "بدون راتب")]
    WithoutSallary
}
