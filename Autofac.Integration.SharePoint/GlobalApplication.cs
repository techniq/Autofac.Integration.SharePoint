using System;
using System.IO;
using Autofac.Integration.Web;
using Microsoft.SharePoint.ApplicationRuntime;

namespace Autofac.Integration.SharePoint
{
    public class GlobalApplication : SPHttpApplication, IContainerProviderAccessor
    {
        #region IContainerProviderAccessor Members

        public IContainerProvider ContainerProvider
        {
            get { return SPServiceLocator.ContainerProvider; }
        }

        #endregion

        protected void Application_Start(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;

            string webConfig = Server.MapPath("~/web.config");
            if (File.Exists(webConfig))
                SPServiceLocator.InitializeContainer();
            else
            {
                webConfig = "n/a";
                SPServiceLocator.InitializeContainer();
            }

            DateTime time2 = DateTime.Now;
            Application["_autofac_config"] = webConfig;
            Application["_autofac_init"] = time2.Subtract(time).TotalMilliseconds + " ms";
        }

        protected void Application_End(object sender, EventArgs e)
        {
            SPServiceLocator.Kill();
        }
    }
}