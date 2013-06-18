using System;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    internal class AutofacEnabledChecker
    {
        private static object di_enabled_lock = new object();
        private static bool di_enabled_initialized = false;
        private static bool di_enabled = false;

        /// <summary>
        /// Determines whether the specified web app has autofac dependency injection enabled.
        /// </summary>
        /// <param name="webApp">The web app.</param>
        /// <returns>
        ///   <c>true</c> if the specified web app is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(SPWebApplication webApp)
        {
            if(webApp == null)
                throw new ArgumentNullException("webApp");

            if (!di_enabled_initialized)
            {
                lock (di_enabled_lock)
                {
                    if (!di_enabled_initialized)
                    {
                        di_enabled_initialized = true;
                        if (webApp.Properties.ContainsKey(Constants.AUTOFAC_DI_ENABLED))
                            di_enabled = true;
                    }
                }
            }

            return di_enabled;
        }
    }
}
