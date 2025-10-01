using Android.App;
using Android.OS;
using Android.Runtime;
using PersistentTimerApp.Platforms.Android.Services;

namespace PersistentTimerApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    TimerService.NOTIFICATION_CHANNEL_ID,
                    "Timer Service Channel",
                    NotificationImportance.Default);

                channel.Description = "کانال برای سرویس تایمر";

                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}