using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Lusa.AddinEngine;
using Lusa.AddinEngine.Extension;
using UIShell.OSGi;

namespace AppStartUp
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        static SplashWindow _instance;
        public static SplashWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SplashWindow();
                    _instance.SetProgress(10, "Start to loading plugins");
                    if (AddinEngineHost.Runtime.IsNotNull())
                    {
                        Application.Current.Startup += Current_Startup;
                        Application.Current.LoadCompleted += Current_LoadCompleted;
                        Application.Current.Activated += Current_Activated;
                        AddinEngineHost.Runtime.Framework.EventManager.AddBundleEventListener(_instance.BundleHandler, true);
                        AddinEngineHost.Runtime.Framework.EventManager.AddFrameworkEventListener(_instance.FrameworkHandler);
                    }
                }
                return _instance;
            }
        }

        static void Current_Activated(object sender, EventArgs e)
        {
            
        }

        static void Current_LoadCompleted(object sender, NavigationEventArgs e)
        {
            
        }

        static void Current_Startup(object sender, StartupEventArgs e)
        {
            
        }

        private void FrameworkHandler(object sender, FrameworkEventArgs frameworkEventArgs)
        {
            if (frameworkEventArgs.EventType == FrameworkEventType.Started)
            {
                SetProgress(80, "Plugin Framework Started.");
            }
        }

        public int CurrentProgress
        {
            get { return (int)this.ProgressBar.Value; }
        }

        private void BundleHandler(object sender, BundleStateChangedEventArgs bundleStateChangedEventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                if (ProgressBar.Value < 80)
                {
                    var msg = "Loding plugin " + bundleStateChangedEventArgs.Bundle.Name + ", version :" +
                              bundleStateChangedEventArgs.Bundle.Version;
                    SetProgress((int)ProgressBar.Value + 1, msg);
                }
            });
        }

        public void SetProgress(int progress, string msg)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = progress;
                SetMessage(msg,false);
                if (progress >= 100)
                {
                    this.Close();
                }
            });
        }

        public void SetMessage(string msg,bool dispatch=true)
        {
            var finalMsg = this.CurrentProgress + "% : " + msg;
            if (dispatch)
            {
                Dispatcher.Invoke(() =>
                {
                    TextBlock.Text = finalMsg;
                    System.Windows.Forms.Application.DoEvents();
                });
            }
            else
            {
                TextBlock.Text = finalMsg;
                System.Windows.Forms.Application.DoEvents();
            }
        }

    }
}
