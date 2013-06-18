using System;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint.Installer.Features.AutofacIntegration
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("9578f389-4e0b-40ef-9e8e-5358003e64d4")]
    public class AutofacIntegrationEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // generally equivalent to: SPFarm.Local.Services.GetValue<SPWebService>()
            var webService = properties.Feature.Parent as SPWebService;
            if (webService == null)
            {
                throw new ArgumentNullException("properties.Feature.Parent");
            }

            // let's make the necessary changes to register autofac in web.config and global.asax
            foreach (var webApp in webService.WebApplications)
            {
                if (webApp.IsAdministrationWebApplication)
                    continue; // don't mess with central admin :-)

                // assuming every other web application "wants" to use Autofac
                // (this may not be the case, change logic here, or scope the feature differently)
                AutofacIntegrationInstaller.Install(webApp);

                webApp.WebService.Update();
                webApp.WebService.ApplyWebConfigModifications();
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            var webService = properties.Feature.Parent as SPWebService;
            if (webService == null)
            {
                // allow de-activation here
                return;
            }

            foreach (var webApplication in webService.WebApplications)
            {
                AutofacIntegrationInstaller.Uninstall(webApplication);

                // Reapply all the configuration modifications
                webApplication.WebService.Update();
                webApplication.WebService.ApplyWebConfigModifications();
           }
        }
    }
}
