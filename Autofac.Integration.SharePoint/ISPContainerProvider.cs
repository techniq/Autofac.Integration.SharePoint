namespace Autofac.Integration.SharePoint
{
    public interface ISPContainerProvider
    {
        bool RequestLifetimeIsUnavailable { get; }
        ILifetimeScope NewDisposableLifetime();
        ILifetimeScope NewDisposableLifetime(string name);
        ILifetimeScope GetRequestLifetime();
    }
}