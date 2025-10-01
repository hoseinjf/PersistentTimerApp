using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Timers;
using Timer = System.Timers.Timer;
using Android.Content.PM;

namespace PersistentTimerApp.Platforms.Android.Services
{

    [Service(Name = "com.companyname.persistenttimerapp.Services.TimerService",
             ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class TimerService : Service
    {
        private Timer _timer;
        private int _seconds;
        public const string NOTIFICATION_CHANNEL_ID = "10001";
        private const int NOTIFICATION_ID = 1;
        private bool isForeground = false;

        public override IBinder OnBind(Intent intent) => null;

        public override void OnCreate()
        {
            base.OnCreate();
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _seconds = Preferences.Get("timer_seconds", 0);

            if (intent.Action != null)
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

        private void StartTimer()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
                StartAsForeground();
            }
        }

        private void StopTimer()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _seconds = 0;
            Preferences.Set("timer_seconds", _seconds);
            StopAsForeground();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _seconds++;
            Preferences.Set("timer_seconds", _seconds);
            UpdateNotification();
        }

        private void StartAsForeground()
        {
            var notification = CreateNotification();
            StartForeground(NOTIFICATION_ID, notification);
            isForeground = true;
        }

        private void StopAsForeground()
        {
            StopForeground(true);
            isForeground = false;
        }

        private void UpdateNotification()
        {
            if (isForeground)
            {
                var notification = CreateNotification();
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(NOTIFICATION_ID, notification);
            }
        }

        private Notification CreateNotification()
        {
            TimeSpan time = TimeSpan.FromSeconds(_seconds);
            string timeString = time.ToString(@"hh\:mm\:ss");

            var notificationBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("Timer Running")
                .SetContentText($"Elapsed Time: {timeString}")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true);

            return notificationBuilder.Build();
        }

        public override void OnDestroy()
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            base.OnDestroy();
        }
    }
}

