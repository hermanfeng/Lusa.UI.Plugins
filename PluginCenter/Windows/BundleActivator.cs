using System;
using System.Windows;
using UIShell.BundleManagementService;
using UIShell.OSGi;
using UIShell.OSGi.Core.Service;

namespace UIShell.WpfAppCenterPlugin
{
	public class BundleActivator : IBundleActivator
	{
		public static IBundle Bundle
		{
			get;
			private set;
		}

		public static IBundleInstallerService BundleInstallerService
		{
			get;
			private set;
		}

		public static ServiceTracker<IBundleManagementService> BundleManagementServiceTracker
		{
			get;
			private set;
		}

		public void Start(IBundleContext context)
		{
			Bundle = context.Bundle;
			BundleManagementServiceTracker = new ServiceTracker<IBundleManagementService>(context);
			BundleInstallerService = context.GetFirstOrDefaultService<IBundleInstallerService>();
			Application firstOrDefaultService = context.GetFirstOrDefaultService<Application>();
			if (firstOrDefaultService != null)
			{
				try
				{
					ResourceDictionary resourceDictionary = new ResourceDictionary();
					resourceDictionary.MergedDictionaries.Add(new ResourceDictionary
					{
						Source = new Uri("/FirstFloor.ModernUI,Version=1.0.3.0;component/Assets/ModernUI.xaml", UriKind.RelativeOrAbsolute)
					});
					resourceDictionary.MergedDictionaries.Add(new ResourceDictionary
					{
						Source = new Uri("/FirstFloor.ModernUI,Version=1.0.3.0;component/Assets/ModernUI.Light.xaml", UriKind.RelativeOrAbsolute)
					});
					firstOrDefaultService.Resources = resourceDictionary;
				}
				catch
				{
				}
			}
		}

		public void Stop(IBundleContext context)
		{
		}
	}
}
