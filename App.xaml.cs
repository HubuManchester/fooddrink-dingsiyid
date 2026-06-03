namespace CampusEats
{
    public partial class App : Application
    {
        public App()
        {
            // 添加未处理异常处理（使用 Debug.WriteLine 避免文件操作）
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"[CRASH] Unhandled exception: {ex?.ToString() ?? "Unknown exception"}");
            };
            
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH] Unobserved task exception: {e.Exception.ToString()}");
                e.SetObserved();
            };
            
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