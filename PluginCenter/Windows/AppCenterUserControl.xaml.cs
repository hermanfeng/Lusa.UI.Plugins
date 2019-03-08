using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UIShell.BundleManagementService;
using UIShell.iOpenWorks.BundleRepository.OpenAPI;

namespace UIShell.WpfAppCenterPlugin
{
    public partial class AppCenterUserControl : UserControl
	{

		private string KeyWord
		{
			get;
			set;
		}

		private List<BundleDetails> Bundles
		{
			get;
			set;
		}

		public AppCenterUserControl()
		{
			InitializeComponent();
			LoadBundles();
		}

		private void InstallButton_Click(object sender, RoutedEventArgs e)
		{
			if (BundlesDataGrid.SelectedItems.Count > 0)
			{
				List<RequestBundle> list = new List<RequestBundle>();
				foreach (BundleDetails selectedItem in BundlesDataGrid.SelectedItems)
				{
					list.Add(new RequestBundle
					{
						InputBundleID = selectedItem.BundleID.ToString(),
						SymbolicName = selectedItem.SymbolicName,
						Version = selectedItem.Version,
						Name = selectedItem.Name,
						Upgrade = selectedItem.HasNewVersion
					});
				}
				InstallBundlesWindow installBundlesWindow = new InstallBundlesWindow(list);
				installBundlesWindow.Closed += InstallBundlesWindowClosed;
				installBundlesWindow.ShowDialog();
			}
			else
			{
                MessageBox.Show("请选择需要安装的插件。", "选择插件", MessageBoxButton.OK);
			}
		}

		private void InstallBundlesWindowClosed(object sender, EventArgs e)
		{
			RefreshButton_Click(null, null);
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Key.Text = string.Empty;
			LoadBundles();
		}

		private void LoadBundles()
		{
			if (BundleActivator.BundleManagementServiceTracker.IsServiceAvailable)
			{
				BundlesDataGrid.DataContext = null;
				RefreshButton.IsEnabled = false;
				InstallButton.IsEnabled = false;
				LoadBundlesProgressBar.IsIndeterminate = true;
				LoadBundlesProgressBar.Visibility = Visibility.Visible;
				BundlesDataGrid.Visibility = Visibility.Hidden;
				new Thread((ThreadStart)delegate
				{
					AppCenterUserControl appCenterUserControl = this;
					IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
					List<BundleDetails> bundles = null;
					try
					{
						bundles = defaultOrFirstService.GetRepositoryBundles();
					}
					catch
					{
					}
					Action method = delegate
					{
						appCenterUserControl.LoadBundlesProgressBar.IsIndeterminate = false;
						appCenterUserControl.LoadBundlesProgressBar.Visibility = Visibility.Hidden;
						appCenterUserControl.Bundles = bundles;
						appCenterUserControl.BindBundles();
						appCenterUserControl.BundlesDataGrid.Visibility = Visibility.Visible;
						appCenterUserControl.RefreshButton.IsEnabled = true;
						appCenterUserControl.InstallButton.IsEnabled = true;
					};
					base.Dispatcher.Invoke(method, new object[0]);
				}).Start();
			}
		}

		private void Key_TextChanged(object sender, TextChangedEventArgs e)
		{
			KeyWord = Key.Text.ToLower();
			BindBundles();
		}

		private void BindBundles()
		{
			if (Bundles != null)
			{
				BundlesDataGrid.DataContext = (string.IsNullOrEmpty(KeyWord) ? Bundles : Bundles.FindAll(delegate(BundleDetails b)
				{
					if (!b.Name.ToLower().Contains(KeyWord))
					{
						return b.SymbolicName.ToLower().Contains(KeyWord);
					}
					return true;
				}));
			}
		}

		private void BundlesDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
			while (dependencyObject != null && !(dependencyObject is DataGridRow))
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			}
			if (dependencyObject != null && dependencyObject is DataGridRow)
			{
				DataGridRow dataGridRow = dependencyObject as DataGridRow;
				dataGridRow.IsSelected = !dataGridRow.IsSelected;
				e.Handled = true;
			}
		}

		private void BundleDetailsButton_Click(object sender, RoutedEventArgs e)
		{
			BundleDetails bundleDetails = ((FrameworkElement)sender).DataContext as BundleDetails;
			Process.Start(bundleDetails.DetailsUrl);
		}
	}
}
