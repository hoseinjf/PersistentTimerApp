using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Timers;
using Timer = System.Timers.Timer;
using Android.Content.PM;

namespace PersistentTimerApp.Platforms.Android.Services
{
    // این کلاس جدید، مسئول دریافت "زنگ هشدار" از AlarmManager است
    [BroadcastReceiver(Enabled = true)]
    public class RestartReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // پس از دریافت هشدار، سرویس را دوباره راه‌اندازی می‌کند
            var serviceIntent = new Intent(context, typeof(TimerService));
            serviceIntent.SetAction("ACTION_START");
            context.StartForegroundService(serviceIntent);
        }
    }

    [Service(Name = "com.companyname.persistenttimerapp.Services.TimerService",
             ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class TimerService : Service
    {
        private Timer _timer;
        private int _seconds;
        public const string NOTIFICATION_CHANNEL_ID = "10001";
        private const int NOTIFICATION_ID = 1;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _seconds = Preferences.Get("timer_seconds", 0);

            if (intent?.Action != null)
            {
                switch (intent.Action)
                {
                    case "ACTION_START":
                        StartTimer();
                        break;
                    case "ACTION_STOP":
                        StopTimer();
                        break;
                    case "ACTION_RESET":
                        ResetTimer();
                        break;
                }
            }
            return StartCommandResult.Sticky;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            // اگر تایمر در حال اجرا بود، یک زنگ هشدار برای ۲ ثانیه بعد تنظیم کن
            if (_timer != null && _timer.Enabled)
            {
                var restartServiceIntent = new Intent(this, typeof(RestartReceiver));
                var pendingIntent = PendingIntent.GetBroadcast(this, 1, restartServiceIntent, PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);
                var alarmService = (AlarmManager)GetSystemService(AlarmService);
                alarmService.SetExact(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 2000, pendingIntent);
            }
            base.OnTaskRemoved(rootIntent);
        }

        private void StartTimer()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
            StartForeground(NOTIFICATION_ID, CreateNotification());
        }

        private void StopTimer()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
            StopForeground(true);
            CancelRestartAlarm();
        }

        private void ResetTimer()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
            _seconds = 0;
            Preferences.Set("timer_seconds", _seconds);
            UpdateNotification();
            StopForeground(true);
            CancelRestartAlarm();
        }

        private void CancelRestartAlarm()
        {
            // اگر کاربر خودش تایمر را متوقف کرد، زنگ هشدار را لغو می‌کنیم
            var intent = new Intent(this, typeof(RestartReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(this, 1, intent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
            if (pendingIntent != null)
            {
                var alarmManager = (AlarmManager)GetSystemService(AlarmService);
                alarmManager.Cancel(pendingIntent);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _seconds++;
            Preferences.Set("timer_seconds", _seconds);
            UpdateNotification();
        }

        private void UpdateNotification()
        {
            var notification = CreateNotification();
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(NOTIFICATION_ID, notification);
        }

        private Notification CreateNotification()
        {
            TimeSpan time = TimeSpan.FromSeconds(_seconds);
            string timeString = time.ToString(@"hh\:mm\:ss");

            var notificationBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("تایمر در حال اجراست")
                .SetContentText($"زمان سپری شده: {timeString}")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true);

            return notificationBuilder.Build();
        }

        public override void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
            }
            base.OnDestroy();
        }
    }
}

