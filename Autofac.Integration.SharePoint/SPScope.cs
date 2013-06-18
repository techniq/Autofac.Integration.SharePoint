using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.SharePoint
{
    public enum SPScope
    {
        SPFarm = 1,
        SPWebApplication = 2, 
        SPSite = 4,
        SPWeb = 8
    }
}
