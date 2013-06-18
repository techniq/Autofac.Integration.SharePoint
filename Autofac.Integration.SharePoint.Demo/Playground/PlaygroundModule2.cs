using Autofac.Builder;

namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public class PlaygroundModule2 : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // InstancePerLifetimeScope implementation
            //builder.Register(c => new PlayInterface(Guid.NewGuid().ToString("N"))).As<IPlayInterface>().InstancePerLifetimeScope(); // InstancePerHttpRequest (httpRequest) lifetime scope default
            builder.RegisterType<PlayInterface>().As<IPlayInterface>().InstancePerLifetimeScope();
                // InstancePerHttpRequest (httpRequest) lifetime scope default
            builder.RegisterType<PlaygroundPresenter>().AsSelf().InstancePerDependency();
            builder.RegisterGeneratedFactory<PlaygroundPresenterFactory>();
        }
    }
}