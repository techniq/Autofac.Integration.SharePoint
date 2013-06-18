using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Security;

namespace Autofac.Integration.SharePoint.Installer.Features.AutofacWebAppIntegration
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("e8d1d756-dfae-41d0-8aa0-f6711a111e06")]
    public class AutofacWebAppIntegrationEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // generally equivalent to: SPFarm.Local.Services.GetValue<SPWebService>()
            var webApp = properties.Feature.Parent as SPWebApplication;
            if (webApp == null)
                throw new ArgumentNullException("properties.Feature.Parent");


            // assuming every other web application "wants" to use Autofac
            // (this may not be the case, change logic here, or scope the feature differently)
            AutofacIntegrationInstaller.Install(webApp);

            webApp.WebService.Update();
            webApp.WebService.ApplyWebConfigModifications();
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            var webApp = properties.Feature.Parent as SPWebApplication;
            if (webApp == null)
            {
                // allow de-activation here
                return;
            }

            AutofacIntegrationInstaller.Uninstall(webApp);

            // Reapply all the configuration modifications
            webApp.WebService.Update();
            webApp.WebService.ApplyWebConfigModifications();
        }
    }
}
