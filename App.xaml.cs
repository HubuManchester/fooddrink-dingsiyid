namespace CampusEats
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            Task.Run(async () => await AppState.InitializeAsync());
        }

        protected override void OnSleep()
        {
            if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.Stop();
            }
        }

        protected override void OnResume()
        {
            // Accelerometer will be restarted by the HardwareManager when needed
        }
    }
}