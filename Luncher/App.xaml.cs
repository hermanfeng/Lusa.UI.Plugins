using System;
using System.Diagnostics;
using System.Windows;
using Lusa.AddinEngine;
using Lusa.UI.Msic.MessageService;
using Lusa.UI.WorkBenchContract;
using UIShell.OSGi;
using UIShell.OSGi.Logging;
using UIShell.OSGi.Utility;

namespace AppStartUp
{
    /// <summary>
    /// WPF startup class.
    /// </summary>
    public partial class App : Application,IStartupReporter
    {
        private BundleRuntime _bundleRuntime;
        // Use object type to avoid load UIShell.OSGi.dll before update.

        [STAThreadAttribute]
        public static void Main()
        {
            var app = new App();
            app.Start();
        }

        private void Start()
        {
            StartBundleRuntime();
        }

        void StartBundleRuntime() // Start OSGi Core.
        {
            FileLogUtility.SetLogLevel(LogLevel.Debug);
            FileLogUtility.SetMaxFileSizeByMB(10);
            FileLogUtility.SetCreateNewFileOnMaxSize(true);
            var st = new Stopwatch();
            st.Start();
            var setting = new AddinEngineStartUpSetting();
            AddinEngineHost.InitializeBundleRuntime(setting);

            SplashWindow.Instance.Show();

            var bundleRuntime = AddinEngineHost.Runtime;
            bundleRuntime.AddService<Application>(this);
            AddinEngineHost.StartRuntime();

            Exit += AppExit;
            _bundleRuntime = bundleRuntime;
            st.Stop();
            MessageService.Instance.SendMessage("StartRuntime takes "+st.ElapsedMilliseconds+"ms");
            StartupWorkbench();
        }

        void StartupWorkbench()
        {
            var bundleRuntime = AddinEngineHost.Runtime;
            Application app = Application.Current;
            app.DispatcherUnhandledException += app_DispatcherUnhandledException;
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            app.ShutdownMode = ShutdownMode.OnLastWindowClose;
            var bench = bundleRuntime.GetFirstOrDefaultService<IWorkBench>();
            if (bench != null)
            {
                var env = new WorkBenchEnviroment();
                var startsetting = new WorkBenchStartupSetting();
                startsetting.StartupReporter = this;
                SplashWindow.Instance.SetProgress(70,"Initialize workbench.");
                bench.Initialize(env);
                SplashWindow.Instance.SetProgress(90, "Run workbench.");
                bench.Run(startsetting);
            }
            else
            {
                SplashWindow.Instance.SetProgress(90, "Can not find workbench.");
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageService.Instance.SendMessage(e.ExceptionObject as Exception);
        }

        void app_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageService.Instance.SendMessage(e.Exception);
        }

        void AppExit(object sender, ExitEventArgs e)
        {
            if (_bundleRuntime != null)
            {
                var bundleRuntime = _bundleRuntime as BundleRuntime;
                bundleRuntime.Stop();
                _bundleRuntime = null;
            }
        }

        int IStartupReporter.CurrentProgress
        {
            get { return SplashWindow.Instance.CurrentProgress; }
        }

        void IStartupReporter.Report(int progress, string msg)
        {
            SplashWindow.Instance.SetProgress(progress,msg);
        }
    }
}
