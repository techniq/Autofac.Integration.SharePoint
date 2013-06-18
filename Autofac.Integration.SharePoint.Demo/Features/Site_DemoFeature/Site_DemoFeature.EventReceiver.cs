using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Autofac.Integration.SharePoint.Demo.Playground;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;

namespace Autofac.Integration.SharePoint.Demo.Features.Site_DemoFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("d96d3f23-22d7-4de4-ba06-73241b9be32a")]
    public class Site_DemoFeatureEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // let's register an autofac module (saves the config in the farm properties)
            SPContainerBuilder.RegisterModule<SiteLevelModule>((SPSite)properties.Feature.Parent);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPContainerBuilder.RemoveModule<SiteLevelModule>((SPSite)properties.Feature.Parent);
        }
    }
}
