using System;
using System.IO;
using System.Web;
using Microsoft.SharePoint;

namespace Autofac.Integration.SharePoint
{
    /// <summary>
    /// HttpModule that handles initialization of SPServiceLocator for Autofac
    /// </summary>
    public class ServiceLocatorInitializationModule : IHttpModule
    {
        public void Init(HttpApplication httpApp)
        {
            //DateTime start = DateTime.Now;

            //string webConfig = httpApp.Context.Server.MapPath("~/web.config");
            //if (File.Exists(webConfig))
            //    SPServiceLocator.InitializeContainer(webConfig);
            //else
            //{
            //    webConfig = null;
            //    SPServiceLocator.InitializeContainer();
            //}

            //DateTime end = DateTime.Now;
            //httpApp.Application["_autofac_config"] = webConfig;
            //httpApp.Application["_autofac_init"] = end.Subtract(start).TotalMilliseconds + " ms";
        }

        public void Dispose()
        {
            SPServiceLocator.Kill();
        }
    }
}
