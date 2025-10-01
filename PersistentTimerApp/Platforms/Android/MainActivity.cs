using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace PersistentTimerApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const int NOTIFICATION_PERMISSION_REQUEST_CODE = 101;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CheckAndRequestNotificationPermission();
        }

        private void CheckAndRequestNotificationPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (ContextCompat.CheckSelfPermission(this, "android.permission.POST_NOTIFICATIONS") != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[] { "android.permission.POST_NOTIFICATIONS" }, NOTIFICATION_PERMISSION_REQUEST_CODE);
                }
            }
        }
    }
}