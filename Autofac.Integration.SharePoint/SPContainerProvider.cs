using System;
using System.Web;

namespace Autofac.Integration.SharePoint
{
    public class SPContainerProvider : ContainerProvider, ISPContainerProvider
    {
        public SPContainerProvider(IContainer applicationContainer) : base(applicationContainer)
        {
        }

        public SPContainerProvider(IContainer applicationContainer,
                                   Action<ContainerBuilder> requestLifetimeConfiguration)
            : base(applicationContainer, requestLifetimeConfiguration)
        {
        }

        #region ISPContainerProvider Members

        public bool RequestLifetimeIsUnavailable
        {
            get { return HttpContext.Current == null; }
        }

        public ILifetimeScope NewDisposableLifetime()
        {
            return ApplicationContainer.BeginLifetimeScope();
        }

        public ILifetimeScope NewDisposableLifetime(string name)
        {
            return ApplicationContainer.BeginLifetimeScope(name);
        }

        public ILifetimeScope GetRequestLifetime()
        {
            return !RequestLifetimeIsUnavailable ? RequestLifetime : null;
        }

        #endregion
    }
}