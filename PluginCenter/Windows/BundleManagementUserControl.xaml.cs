using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using UIShell.BundleManagementService;
using UIShell.OSGi;

namespace UIShell.WpfAppCenterPlugin
{
	public partial class BundleManagementUserControl : UserControl
	{
		public BundleManagementUserControl()
		{
			InitializeComponent();
		}

		private void BindBundlesData()
		{
			IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
			if (defaultOrFirstService != null)
			{
				List<IBundle> localBundles = defaultOrFirstService.GetLocalBundles();
				int selectedIndex = BundlesDataGrid.SelectedIndex;
				BundlesDataGrid.DataContext = null;
				BundlesDataGrid.DataContext = new List<IBundle>(localBundles);
				if (localBundles.Count > 0)
				{
					BundlesDataGrid.SelectedIndex = ((selectedIndex >= 0) ? selectedIndex : 0);
				}
				else
				{
					BundlesDataGrid.SelectedItem = null;
				}
				SelectBundle();
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			BindBundlesData();
		}

		private void BundlesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectBundle();
		}

		private void SelectBundle()
		{
			object selectedItem = BundlesDataGrid.SelectedItem;
			IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
			if (selectedItem != null && defaultOrFirstService != null)
			{
				IBundle bundle = selectedItem as IBundle;
				bool isEnabled = defaultOrFirstService.IsBundleAllowedToStart(bundle.BundleID);
				bool isEnabled2 = defaultOrFirstService.IsBundleAllowedToStop(bundle.BundleID);
				bool isEnabled3 = defaultOrFirstService.IsBundleAllowedToUninstall(bundle.BundleID);
				StartButton.IsEnabled = isEnabled;
				StopButton.IsEnabled = isEnabled2;
				UninstallButton.IsEnabled = isEnabled3;
				StartButton.Visibility = ((bundle.State == BundleState.Active) ? Visibility.Hidden : Visibility.Visible);
				StopButton.Visibility = ((bundle.State != BundleState.Active) ? Visibility.Hidden : Visibility.Visible);
				UninstallButton.Visibility = Visibility.Visible;
				SelectedBundleTextBlock.Text = $"当前插件：{bundle.Name}";
			}
			else
			{
				SelectedBundleTextBlock.Text = string.Empty;
				StartButton.Visibility = Visibility.Hidden;
				StopButton.Visibility = Visibility.Hidden;
				UninstallButton.Visibility = Visibility.Hidden;
			}
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			object selectedItem = BundlesDataGrid.SelectedItem;
			IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
			if (selectedItem != null && defaultOrFirstService != null)
			{
				try
				{
					defaultOrFirstService.StartBundle((selectedItem as IBundle).BundleID);
				}
				catch (BundleException)
				{
				}
				BindBundlesData();
			}
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			object selectedItem = BundlesDataGrid.SelectedItem;
			IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
			if (selectedItem != null && defaultOrFirstService != null)
			{
				try
				{
					defaultOrFirstService.StopBundle((selectedItem as IBundle).BundleID);
				}
				catch (BundleException)
				{
				}
				BindBundlesData();
			}
		}

		private void UninstallButton_Click(object sender, RoutedEventArgs e)
		{
			object selectedItem = BundlesDataGrid.SelectedItem;
			IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
			if (selectedItem != null && defaultOrFirstService != null)
			{
				try
				{
					defaultOrFirstService.UninstallBundle((selectedItem as IBundle).BundleID);
				}
				catch (BundleException)
				{
				}
				BindBundlesData();
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			BindBundlesData();
		}
	}
}
