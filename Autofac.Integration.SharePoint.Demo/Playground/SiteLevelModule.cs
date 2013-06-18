using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public class SiteLevelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SiteLevelDependency>().As<ISiteLevelDependency>().InstancePerLifetimeScope();
        }
    }
}
