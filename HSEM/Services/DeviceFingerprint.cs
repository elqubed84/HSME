using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace HSEM.Services
{

    public static class DeviceFingerprint
    {
        private const string DeviceIdKey = "DEVICE_UNIQUE_ID";

        public static async Task<string> GetDeviceIdAsync()
        {
            // أولاً نحاول نقرأ الرقم من SecureStorage
            try
            {
                var storedId = await SecureStorage.Default.GetAsync(DeviceIdKey);
                if (!string.IsNullOrEmpty(storedId))
                    return storedId;
            }
            catch
            {
                // ممكن SecureStorage ما يشتغلش على بعض الأجهزة القديمة
            }

            // لو مفيش، نولد رقم جديد حسب النظام
            string deviceId = null;

#if ANDROID
            deviceId = Android.Provider.Settings.Secure.GetString(
                Android.App.Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
#elif IOS
        deviceId = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.AsString();
#else
        // للـ Windows أو غيرها، نولد UUID
        deviceId = Guid.NewGuid().ToString();
#endif

            // إذا حصل أي مشكلة، نضمن وجود UUID
            if (string.IsNullOrEmpty(deviceId))
                deviceId = Guid.NewGuid().ToString();

            // نخزن الرقم في SecureStorage للاستخدام المستقبلي
            try
            {
                await SecureStorage.Default.SetAsync(DeviceIdKey, deviceId);
            }
            catch
            {
                // ممكن الفشل يحصل لو الجهاز مش داعم
            }

            return deviceId;
        }
    }

}
