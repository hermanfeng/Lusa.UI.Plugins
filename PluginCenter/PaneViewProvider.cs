using Lusa.UI.WorkBenchContract.Controls.Pane;

namespace PluginCenter
{
    public class PluginCenterPanViewProvider : IPaneViewDescriptorProvider
    {
        PaneViewDescriptor IPaneViewDescriptorProvider.Pane
        {
            get
            {
                return new PaneViewDescriptor(typeof(PaneViewUserControl)) { Header = "PluginManager", IsDocument = true };
            }
        }
    }
}
