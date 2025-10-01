#if ANDROID
using Android.Content;
using PersistentTimerApp.Platforms.Android.Services;
#endif

namespace PersistentTimerApp
{
    public partial class MainPage : ContentPage
    {
        private IDispatcherTimer _uiTimer;

        public MainPage()
        {
            InitializeComponent();
            SetupUiTimer();
        }

        private void SetupUiTimer()
        {
            _uiTimer = Dispatcher.CreateTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += (s, e) => UpdateLabel();
            _uiTimer.Start();
        }

        private void UpdateLabel()
        {
            int seconds = Preferences.Get("timer_seconds", 0);
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            TimerLabel.Text = time.ToString(@"hh\:mm\:ss");
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            SendActionToService("ACTION_START");
        }

        private void StopButton_Clicked(object sender, EventArgs e)
        {
            SendActionToService("ACTION_STOP");
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            SendActionToService("ACTION_RESET");
        }

        private void SendActionToService(string action)
        {
#if ANDROID
        var intent = new Intent(global::Android.App.Application.Context, typeof(TimerService));
        intent.SetAction(action);
        global::Android.App.Application.Context.StartForegroundService(intent);
#endif
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateLabel();
        }
    }
}