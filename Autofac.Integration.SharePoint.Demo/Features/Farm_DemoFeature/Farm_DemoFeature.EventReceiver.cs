using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;

namespace Autofac.Integration.SharePoint.Demo.Features.Farm_DemoFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("f3e68d78-4f3a-4c9b-a3ca-12837d1238d9")]
    public class Farm_DemoFeatureEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPContainerBuilder.RegisterModule<Playground.PlaygroundModule>();
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPContainerBuilder.RemoveModule<Playground.PlaygroundModule>();
        }
    }
}
