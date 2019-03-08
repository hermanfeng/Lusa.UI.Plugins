using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UIShell.BundleManagementService;
using UIShell.iOpenWorks.BundleRepository.OpenAPI;

namespace UIShell.WpfAppCenterPlugin
{
    public partial class InstallBundlesWindow : Window, IInstallationProgressReport
	{

		private List<RequestBundle> RequestBundles
		{
			get;
			set;
		}

		private bool IsInstallationInProgress
		{
			get;
			set;
		}

		public InstallBundlesWindow(List<RequestBundle> requestBundles)
		{
			InitializeComponent();
			base.Style = (Style)Application.Current.Resources["EmptyWindow"];
			RequestBundles = requestBundles;
		}

		public void AppendProgressItem(ProgressReportItem item)
		{
			Action method = delegate
			{
				if (!string.IsNullOrEmpty(item.Title))
				{
					TitleTextBlock.Text = item.Title;
				}
				if (!string.IsNullOrEmpty(item.Message))
				{
					MessageTextBlock.Inlines.Add(item.Message);
					MessageTextBlock.Inlines.Add(Environment.NewLine);
					ScrollViewer.LineDown();
					ScrollViewer.LineDown();
				}
				InstallProgressBar.Value = (double)item.Percentage;
			};
			base.Dispatcher.Invoke(method, new object[0]);
		}

		public void ClearProgressReport()
		{
		}

		private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (RequestBundles != null && RequestBundles.Count > 0)
			{
				IBundleManagementService bundleManagementService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
				if (bundleManagementService != null)
				{
					IsInstallationInProgress = true;
					new Thread((ThreadStart)delegate
					{
						bundleManagementService.InstallBundles(RequestBundles, this);
						AppendProgressItem(new ProgressReportItem
						{
							Title = "操作完成，需要重启",
							Message = "系统要求重启，请点击'重启系统'按钮完成重启。",
							Percentage = 100
						});
						IsInstallationInProgress = false;
					}).Start();
				}
			}
		}

		private void ModernWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = IsInstallationInProgress;
		}

		
	}
}
