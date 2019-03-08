using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UIShell.BundleManagementService;
using UIShell.iOpenWorks.BundleRepository.OpenAPI;

namespace UIShell.WpfAppCenterPlugin
{
    public partial class PrivateAppCenterUserControl : UserControl
	{
		public PrivateAppCenterUserControl()
		{
			InitializeComponent();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			string name = UserNameTextBox.Text.Trim();
			string password = PasswordTextBox.Password.Trim();
			if (string.IsNullOrEmpty(name))
			{
                MessageBox.Show("用户名不能为空。", "警告", MessageBoxButton.OK);
				UserNameTextBox.Focus();
			}
			else if (string.IsNullOrEmpty(password))
			{
                MessageBox.Show("用户名不能为空。", "警告", MessageBoxButton.OK);
				UserNameTextBox.Focus();
			}
			else if (BundleActivator.BundleManagementServiceTracker.IsServiceAvailable)
			{
				BundlesDataGrid.DataContext = null;
				LoadBundlesProgressBar.IsIndeterminate = true;
				LoadBundlesProgressBar.Visibility = Visibility.Visible;
				new Thread((ThreadStart)delegate
				{
					bool failed = false;
					IBundleManagementService defaultOrFirstService = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService;
					List<BundleDetails> bundles = new List<BundleDetails>();
					try
					{
						List<ProjectData> projectsByUser = defaultOrFirstService.GetProjectsByUser(name, password);
						ProjectData bundleRepositoryProject = defaultOrFirstService.GetBundleRepositoryProject();
						foreach (ProjectData item in projectsByUser)
						{
							if (!item.ProjectId.Equals(bundleRepositoryProject.ProjectId))
							{
								List<BundleDetails> repositoryBundlesByProject = defaultOrFirstService.GetRepositoryBundlesByProject(item.ProjectId, name, password);
								if (repositoryBundlesByProject != null && repositoryBundlesByProject.Count > 0)
								{
									bundles.AddRange(repositoryBundlesByProject);
								}
							}
						}
					}
					catch
					{
						failed = true;
					}
					Action method = delegate
					{
						if (failed)
						{
                            MessageBox.Show("获取插件失败，请输入正确的帐号并确保网络畅通。", "错误", MessageBoxButton.OK);
						}
						else
						{
							LoadBundlesProgressBar.IsIndeterminate = false;
							LoadBundlesProgressBar.Visibility = Visibility.Hidden;
							if (bundles.Count == 0)
							{
								BundlesDataGrid.Visibility = Visibility.Hidden;
                                MessageBox.Show("插件列表为空。", "提示", MessageBoxButton.OK);
							}
							else
							{
								BundlesDataGrid.Visibility = Visibility.Visible;
								BundlesDataGrid.ItemsSource = null;
								BundlesDataGrid.ItemsSource = bundles;
							}
						}
					};
					base.Dispatcher.Invoke(method, new object[0]);
				}).Start();
			}
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

		private void BundleDetailsButton_Click(object sender, RoutedEventArgs e)
		{
			BundleDetails bundleDetails = ((FrameworkElement)sender).DataContext as BundleDetails;
			Process.Start(bundleDetails.DetailsUrl);
		}
	}
}
