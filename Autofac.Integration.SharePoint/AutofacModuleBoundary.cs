using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    internal interface ISharePointBoundary
    {
        Guid ID { get; }
        IPropertyProvider PropertyProvider { get; }
    }

    internal class SiteBoundary : ISharePointBoundary
    {
        private SPSite _site;
        private IPropertyProvider _propertyProvider;

        public SiteBoundary(SPSite site)
        {
            _site = site;
        }

        public Guid ID
        {
            get { return _site.ID; }
        }

        public IPropertyProvider PropertyProvider
        {
            get
            {
                if (_propertyProvider == null)
                    _propertyProvider = PropertyProviderFactory.GetPropertyProvider(_site);
                return _propertyProvider;
            }
        }

        public override string ToString()
        {
            return "Site " + _site.Url + " (ID=" + _site.ID + ")";
        }
    }

    internal class WebApplicationBoundary : ISharePointBoundary
    {
        private SPWebApplication _webApp;
        private IPropertyProvider _propertyProvider;

        public WebApplicationBoundary(SPWebApplication webApp)
        {
            _webApp = webApp;
        }

        public Guid ID
        {
            get { return _webApp.Id; }
        }

        public IPropertyProvider PropertyProvider
        {
            get
            {
                if (_propertyProvider == null)
                    _propertyProvider = PropertyProviderFactory.GetPropertyProvider(_webApp);
                return _propertyProvider;
            }
        }

        public override string ToString()
        {
            return "Webapplication " + _webApp.Name + " (ID=" + _webApp.Id + ")";
        }
    }

    internal class FarmBoundary : ISharePointBoundary
    {
        private IPropertyProvider _propertyProvider;

        public Guid ID
        {
            get { return Guid.Empty; }
        }

        public IPropertyProvider PropertyProvider
        {
            get
            {
                if (_propertyProvider == null)
                    _propertyProvider = PropertyProviderFactory.GetPropertyProvider();
                return _propertyProvider;
            }
        }

        public override string ToString()
        {
            return "Farm";
        }
    }
}
