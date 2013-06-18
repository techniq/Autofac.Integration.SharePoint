using Autofac.Builder;

namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public class PlaygroundModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // presenters and views
            //builder.RegisterType<UlsLogger>().AsImplementedInterfaces().SingleInstance(); // logger singleton
            builder.RegisterType<UlsLogger>().As<ILogger>().WithParameter("categoryName", "Autofac Logger").
                SingleInstance(); // logger singleton
        }
    }
}